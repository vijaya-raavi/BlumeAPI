using Ontec.Payment.Core.Interfaces;
using Ontec.Payment.Core.Models;

namespace Ontec.Payment.Core
{
    public class PaymentProcessor : IPaymentProcessor
    {
        private readonly IWalletServiceFactory _walletServiceFactory;
        private readonly IRechargeService _rechargeService;

        public PaymentProcessor(IWalletServiceFactory walletServiceFactory, IRechargeService rechargeService)
        {
            _walletServiceFactory=walletServiceFactory;
            _rechargeService=rechargeService;
        }
        public async Task<PaymentWithFeeResponse> CalculateFee(CalculateFeeRequest request)
        {
            var walletService = await _walletServiceFactory.GetWalletServiceInstanceAsync(request.UserId);
            //1. Get user wallet balance
            var walletBalance = await walletService.GetBalanceAsync();
            //2. add wallet balance to topup amount and apply fee
            var processingFeeInPercent = 5; //need to read this from configuration
            var rechargeBreakup = FeeHelper.GetRechargeBreakup(walletBalance, request.Amount, processingFeeInPercent);
            //3.return response with fee and remaining amount breakup
            var response = new PaymentWithFeeResponse()
            {
                BaseAmount = rechargeBreakup.Amount,
                Fee = rechargeBreakup.Fee,
                WalletBalance = walletBalance
            };

            response.Checksum = ChecksumHelper.PopulateChecksum($"{response.BaseAmount}{response.Fee}"); //Adding this checksum, this needs to be passed to processPaymentRequest and to be validated by application

            return response;
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            var walletService = await _walletServiceFactory.GetWalletServiceInstanceAsync(request.UserId);
            var transactionReferenceNumber = Guid.NewGuid().ToString();
            var checkSumToValidate = ChecksumHelper.PopulateChecksum($"{request.PaymentBreakup.Amount}{request.PaymentBreakup.Fee}");
            if (checkSumToValidate == request.PreviousChecksum)
            {
                //1. Check for wallet balance
                var walletBalance = await walletService.GetBalanceAsync();

                if (walletBalance>0)
                {
                    //3. Deduct wallet balance and add deposited amount in wallet amount
                    await walletService.DeductBalanceAsync(request.TransactionAmount,walletBalance);
                }

                var rechargeBreakup = request.PaymentBreakup;

                //4. Recharge on master api

                var response = await _rechargeService.RechargeAsync(new RechargeRequest()
                {
                    MeterNumber = request.MeterNumber,
                    Amount = rechargeBreakup.Amount
                });

                if (response.Status == "successful")
                {
                    //5. If recharge successful save recharge transaction in db
                    await _rechargeService.SaveTransactionAsync(transactionReferenceNumber, response);
                    return new ProcessPaymentResponse()
                    {
                        Status = "successful",
                        TransactionReferenceNumber = transactionReferenceNumber
                    };
                }
                else
                {
                    //6. If recharge failed save failed transaction in db and add the amount in wallet again
                    await _rechargeService.SaveTransactionAsync(transactionReferenceNumber, response);

                    await walletService.RefundAsync(rechargeBreakup.TotalAmount);

                    return new ProcessPaymentResponse()
                    {
                        Status = "failed",
                        TransactionReferenceNumber = transactionReferenceNumber
                    };
                }

            }
            else
            {
                throw new Exception("error validating payment details");
            }
        }
    }
}
