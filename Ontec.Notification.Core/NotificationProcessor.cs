using Ontec.Core.Domain.BankNotification;
using Ontec.Notification.Core.Interfaces;
using Ontec.Payment.Core;
using Ontec.Payment.Core.Interfaces;
using Ontec.Payment.Core.Models;

namespace Ontec.Notification.Core
{
    public class NotificationProcessor : INotificationProcessor
    {
        private INotificationValidator _notificationValidator;
        private IBankNotificationRepository _notificationStore;
        private IWalletServiceFactory _walletServiceFactory;
        private IRechargeService _rechargeService;
        private IUserService _userService;

        public NotificationProcessor(INotificationValidator notificationValidator,
            IBankNotificationRepository notificationStore,
            IWalletServiceFactory walletServiceFactory,
            IRechargeService rechargeService,
            IUserService userService
            )
        {
            _notificationStore = notificationStore;
            _notificationValidator = notificationValidator;
            _rechargeService = rechargeService;
            _userService = userService;
            _walletServiceFactory = walletServiceFactory;

        }

        public async Task ProcessAsync(BankNotificationDetails notification)
        {
            var transactionReferenceNumber = Guid.NewGuid().ToString();
            //1. Validate checksum. If valid proceed for next execution or record error and acknowledge as success/failure to netUp service
            var notificationValidationResult = _notificationValidator.Validate(notification);
            if (notificationValidationResult.IsValid)
            {
                //Store 
                //1. Validate user
                var user = await _userService.GetUserUsingPayerReferenceNumber(notification.PayerReferenceNumber);
                if (user != null)
                {
                    var walletService = await _walletServiceFactory.GetWalletServiceInstanceAsync(user.UserId);
                    //2. Check for wallet balance
                    var walletBalance = await walletService.GetBalanceAsync();
                    var processingFeeInPercent = 5; //need to read this from configuration


                    var rechargeBreakup = FeeHelper.GetRechargeBreakup(walletBalance, notification.TransactionValue, processingFeeInPercent);

                    if (walletBalance > 0)
                    {
                        //3. Deduct wallet balance and add deposited amount in wallet amount
                        await walletService.DeductBalanceAsync(notification.TransactionValue, walletBalance);
                    }
                    //4. Recharge on master api

                    var response = await _rechargeService.RechargeAsync(new RechargeRequest()
                    {
                        MeterNumber = notification.PayerReferenceNumber,
                        Amount = rechargeBreakup.Amount
                    });

                    if (response.Status == "successful")
                    {
                        //5. If recharge successful save recharge transaction in db
                        await _rechargeService.SaveTransactionAsync(transactionReferenceNumber, response);
                    }
                    else
                    {
                        //6. If recharge failed save failed transaction in db and add the amount in wallet again
                        await _rechargeService.SaveTransactionAsync(transactionReferenceNumber, response);

                        await walletService.RefundAsync(rechargeBreakup.TotalAmount);
                    }

                }


            }
            else
            {
                await _notificationStore.SaveErrorAsync(notification);
            }
        }
    }
}
