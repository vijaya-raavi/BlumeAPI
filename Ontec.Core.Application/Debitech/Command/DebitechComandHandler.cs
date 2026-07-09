using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Debitech;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.Debitech.Command;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.TopUp.Queries;
using Ontec.Core.Domain.Requests.Wallet.Command;

namespace Ontec.Core.Application.Debitech.Command
{
    public class DebitechComandHandler : IRequestHandler<BankNotificationRequestQuery, IEnumerable<BankNotificationResponseDto>>
    {
        private readonly IBankNotificationValidator _bankNotificationValidator;
        private readonly ITransactionFeesService _transactionFeesService;
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITopUpRepository _topUpRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IVendRequestHelper _vendRequestHelper;
        private readonly IPushNotification _pushNotification;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyHelper _companyHelper;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IOtpService _otpService;
        private readonly IPropertyUserRepository _propertyUserRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger<DebitechComandHandler> _logger;
        public DebitechComandHandler(IBankNotificationValidator bankNotificationValidator
                                    , IUserRepository userRepository
                                    , IWalletRepository walletRepository
                                    , ITransactionFeesService transactionFeesService
                                    , IPropertyRepository propertyRepository
                                    , IVendRequestHelper vendRequestHelper
                                    , ITopUpRepository topUpRepository
                                    , IPushNotification pushNotification
                                    , ICompanyRepository companyRepository
                                    , ICompanyHelper companyHelper
                                    , INotificationRepository notificationRepository
                                    , ICommunicationRepository communicationRepository
                                    , IOtpService otpService
                                    , IPropertyUserRepository propertyUserRepository
                                    , IMeterRepository meterRepository
                                    , ILogger<DebitechComandHandler> logger
                                    ,IConfigurationRepository configurationRepository)
        {
            _bankNotificationValidator = bankNotificationValidator;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transactionFeesService = transactionFeesService;
            _topUpRepository = topUpRepository;
            _propertyRepository = propertyRepository;
            _vendRequestHelper = vendRequestHelper;
            _pushNotification = pushNotification;
            _companyRepository = companyRepository;
            _companyHelper = companyHelper;
            _notificationRepository = notificationRepository;
            _communicationRepository = communicationRepository;
            _otpService = otpService;
            _meterRepository = meterRepository;
            _propertyUserRepository = propertyUserRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }


        //public async Task<IEnumerable<BankNotificationResponseDto>> Handle(BankNotificationRequestQuery bankRequest, CancellationToken cancellationToken)
        //{
        //    var getPaymentMethodsQuery = new GetPaymentMethodsQuery()
        //    {
        //        StatusId = 0
        //    };
        //    var paymentMethodIds = await _topUpRepository.GetPaymentMethods(getPaymentMethodsQuery).ConfigureAwait(false);
        //    var paymentMethodId = paymentMethodIds.Where(t => t.Slug.ToLower().Contains("bank-transfer", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        //    var response = new List<BankNotificationResponseDto>();
        //    foreach (var request in bankRequest.BankNotificationRequest)
        //    {
        //        string validationMessage = "";
        //        var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
        //        var transactionReferenceNumber = Guid.NewGuid().ToString();
        //        request.TrimAllStrings();
        //        bool isValid = true;
        //        //bool isGuidIdValid = Guid.TryParse(request.NetUpTransactionGuid, out Guid guidOutput);
        //        //if (!isGuidIdValid)
        //        //{
        //        //    validationMessage = "NetUpTransactionGuid is invalid Guid.";
        //        //    isValid = false;
        //        //}
        //        var count = await _topUpRepository.IsNetUpTransactionGuidExist(request.NetUpTransactionGuid).ConfigureAwait(false);
        //        if (count > 0)
        //        {
        //            validationMessage = "NetUpTransactionGuid already exist.";
        //            isValid = false;
        //        }
        //        count = await _topUpRepository.IsBankTransactionIdExist(request.BankTransactionId).ConfigureAwait(false);
        //        if (count > 0)
        //        {
        //            validationMessage = "BankTransactionId already exist.";
        //            isValid = false;
        //        }

        //        var validRequest = _bankNotificationValidator.Validate(request);
        //        if (!validRequest)
        //        {
        //            validationMessage = "NetUpChecksum validation failed.";
        //            isValid = false;
        //        }

        //        if (isValid)
        //        {
        //            var user = await _userRepository.GetUserByPayerReferenceNumber(request.PayerReferenceNumber);
        //            if (user != null)
        //            {
        //                var walletBalance = await _walletRepository.GetBalanceAsync(user.Id);
        //                TransactionFeeDto feeDto = new TransactionFeeDto();

        //                feeDto = await _transactionFeesService.CalculateTransactionFee(paymentMethodId.Id, request.TransactionValue, false, user.Id);

        //                var bankTransferTransaction = new AddBankTransferTransactionDto
        //                {
        //                    Amount = request.TransactionValue,
        //                    TransactionId = strTransactionNumber,
        //                    Flag = (int)PaymentStatus.Complete,
        //                    PaymentMethodId = paymentMethodId.Id,
        //                    PfResponse = JsonConvert.SerializeObject(request),
        //                    RechargeAmount = request.TransactionValue - feeDto.TransactionFee,
        //                    TransactionFee = feeDto.TransactionFee,
        //                    UserId = user.Id,
        //                    MeterId = user.MeterId,
        //                    TopupStatus = TopUpStatusEnum.DebitechSuccess.ToString(),
        //                    UseWallet = 0,
        //                    NetUpTransactionGuid = request.NetUpTransactionGuid,
        //                    BankTransactionId = request.BankTransactionId

        //                };
        //                var transactionId = await _topUpRepository.AddBankTransferTransaction(bankTransferTransaction).ConfigureAwait(false);

        //                if (transactionId > 0)
        //                {
        //                    var payFastModel = new PayFastModel
        //                    {
        //                        merchant_id = "Debitech request",
        //                        m_payment_id = strTransactionNumber,
        //                        payment_status = "COMPLETE",
        //                        pf_payment_id = request.BankTransactionId,
        //                        signature = request.NetUpChecksum
        //                    };

        //                    var result = await _topUpRepository.UpdateTopupTransactions(payFastModel).ConfigureAwait(false);
        //                    var topupTransaction = await _topUpRepository.GetTopupTransactionDetails(payFastModel.m_payment_id);
        //                    if (topupTransaction != null)
        //                    {
        //                        var userDetails = await _userRepository.GetUserById(topupTransaction.UserId).ConfigureAwait(true);
        //                        var userWallet = await _walletRepository.GetUserWalletById(topupTransaction.UserId);
        //                        string transactionRemark = "";
        //                        UpdateWalletBalanceQuery updateWalletBalance = new UpdateWalletBalanceQuery
        //                        {
        //                            UserId = user.Id,
        //                            Amount = topupTransaction.Amount
        //                        };
        //                        //var userWallet = await _walletRepository.GetUserWalletById(updateWalletBalance.UserId);

        //                        //var walletAmountToUse = userWallet.Balance;
        //                        //walletAmountToUse = topupTransaction.Amount >= walletAmountToUse ? walletAmountToUse : topupTransaction.Amount;
        //                        //// double UpdatedBalance = userWallet.Balance - walletAmountToUse;
        //                        //transactionRemark = "Debited after debitech transaction";
        //                        //await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, walletAmountToUse, userWallet.Balance, transactionRemark, false).ConfigureAwait(false);


        //                        //var walletAmountToUse = userWallet.Balance;
        //                        //if (userWallet.Balance > 0)
        //                        //{
        //                        //       walletAmountToUse = topupTransaction.Amount >= walletAmountToUse ? walletAmountToUse : topupTransaction.Amount;
        //                        //        // double UpdatedBalance = userWallet.Balance - walletAmountToUse;
        //                        //        transactionRemark = "Debited after Topup transaction";
        //                        //        await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, walletAmountToUse, userWallet.Balance, transactionRemark, topupTransaction.UseWallet).ConfigureAwait(false);

        //                        //}


        //                        string vendPayMethod = "";
        //                        if (paymentMethodId != null && paymentMethodId.Id > 0)
        //                        {
        //                            vendPayMethod = await _topUpRepository.GetIPayMethodByPaymentMethodId(paymentMethodId.Id).ConfigureAwait(false);
        //                        }
        //                        else
        //                        {
        //                            vendPayMethod = "other";
        //                        }
        //                        var vendResponse = await _vendRequestHelper.ProcessRecharge(topupTransaction, vendPayMethod).ConfigureAwait(true);
        //                        if (vendResponse.StatusCode == 200)
        //                        {
        //                            await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendSuccess.ToString(), (int)PaymentStatus.Complete, vendResponse.Response, transactionId, vendResponse.Token, vendResponse.keyChangeToken, vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif).ConfigureAwait(false);
        //                            var sendReceiptToEmail = new DownloadPurchaceRecieptPdfQuery
        //                            {
        //                                TransactionId = topupTransaction.TransactionID
        //                            };

        //                            // toto: call push notification
        //                            var company = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
        //                            string deviceToken = await _userRepository.GetDeviceToken(user.Id).ConfigureAwait(false);
        //                            var sendMessage = new DownloadPurchaceRecieptPdfQuery
        //                            {
        //                                TransactionId = topupTransaction.TransactionID
        //                            };

        //                            if (!string.IsNullOrEmpty(deviceToken) && deviceToken != "string")
        //                            {
        //                                var PurchaseDetails = await _topUpRepository.GetReceiptDetails(sendMessage).ConfigureAwait(false);
        //                                string title = "Top Up";
        //                                string body = "";
        //                                if (PurchaseDetails != null)
        //                                {
        //                                    if (!string.IsNullOrEmpty(PurchaseDetails.StandardTokens))
        //                                    {

        //                                        var amount = PurchaseDetails.stdamt.ToString().Replace(",", ".");
        //                                        //body = company.Name + " Meter:" + PurchaseDetails.MeterNumber + " RCT: " + PurchaseDetails.stdReceiptId + " Amt: R " + Math.Round(Convert.ToDecimal(PurchaseDetails.stdamt) / 100, 2) + " Token: " + PurchaseDetails.StandardTokens + " Units: " + PurchaseDetails.Units;
        //                                        body = company.Name +
        //                                              "\nMeter: " + PurchaseDetails.MeterNumber +
        //                                              "\nRCT: " + PurchaseDetails.stdReceiptId +
        //                                              "\nAmt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
        //                                              "\nToken: " + PurchaseDetails.StandardTokens +
        //                                              "\nUnits: " + PurchaseDetails.Units;


        //                                    }
        //                                    if (!string.IsNullOrEmpty(PurchaseDetails.BsstToken))
        //                                    {
        //                                        var amount = PurchaseDetails.BsstTokenAmount.ToString().Replace(",", ".");
        //                                        // body = "Payment received. RCT:" + PurchaseDetails.BsstReceiptId + "  Amt: R " + Math.Round(Convert.ToDecimal(PurchaseDetails.BsstTokenAmount) / 100, 2);
        //                                        body = "Payment received." +
        //                                               "\nRCT: " + PurchaseDetails.BsstReceiptId +
        //                                               "\nAmt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2);
        //                                    }
        //                                    else
        //                                    {
        //                                        var amount = PurchaseDetails.Amount.ToString().Replace(",", ".");
        //                                        // body = "Payment received. RCT:" + PurchaseDetails.ReceiptId + "  Amt: R " + Math.Round(Convert.ToDecimal(PurchaseDetails.Amount) / 100, 2) + "Remaining Bal: " + Math.Round(Convert.ToDecimal(PurchaseDetails.RemainingBalance) / 100, 2);
        //                                        body = "Payment received." +
        //                                              "\nRCT: " + PurchaseDetails.ReceiptId +
        //                                              "\nAmt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
        //                                              "\nRemaining Bal: " + Math.Round(Convert.ToDecimal(amount) / 100, 2);
        //                                    }

        //                                    await _pushNotification.SendMessage(title, body, deviceToken, topupTransaction.UserId).ConfigureAwait(false);


        //                                    _ = await _topUpRepository.DownloadPurchaceRecieptPdfQuery(sendReceiptToEmail, user.Id).ConfigureAwait(false);

        //                                }



        //                                AddOrUpdateNotificationsQuery newNotification = new()
        //                                {
        //                                    UserID = topupTransaction.UserId,
        //                                    Title = "Top Up successfull",
        //                                    Description = "Top up successfull, recharged with amount: " + topupTransaction.Amount + " .",
        //                                    IsRead = (int)StatusEnum.Sent,
        //                                    NotificationType = (int)NotificationType.Updated

        //                                };
        //                                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
        //                            }

        //                            response.Add(new BankNotificationResponseDto
        //                            {
        //                                NetUpTransactionGuid = request.NetUpTransactionGuid,
        //                                Message = "Transaction updated successfully.",
        //                                Status = 200
        //                            });
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            transactionRemark = "Credited after Debitech Failed Transaction";
        //                            string title = "Top Up";
        //                            string body = "";
        //                            await _walletRepository.AddBalanceAsync(topupTransaction.UserId, topupTransaction.RechargeAmount, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount,topupTransaction.TransactionID).ConfigureAwait(false);
        //                            AddOrUpdateNotificationsQuery newNotification = new()
        //                            {
        //                                UserID = topupTransaction.UserId,
        //                                Title = "Top Up Failed",
        //                                Description = "Debitech Top up failed Amount : R " + topupTransaction.RechargeAmount + " credited to wallet balance.",
        //                                IsRead = (int)StatusEnum.Sent,
        //                                NotificationType = (int)NotificationType.Updated

        //                            };
        //                            await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
        //                            body = transactionRemark + " Credited Amt : " + topupTransaction.RechargeAmount;
        //                            await SendEmailMessage(topupTransaction, userDetails, body).ConfigureAwait(false);



        //                            //if recharge failed, then added amount to wallet
        //                            //var UpdatedBalance = userWallet.Balance + topupTransaction.Amount;

        //                            //await _walletRepository.UpdateUserWallet(updateWalletBalance, UpdatedBalance);
        //                            //transactionRemark = "Credited after Debitech Failed Transaction";
        //                            //await _walletRepository.AddBalanceAsync(topupTransaction.UserId, topupTransaction.Amount, userWallet.Balance, transactionRemark, false, topupTransaction.Amount).ConfigureAwait(false);

        //                            await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendFailed.ToString(), (int)PaymentStatus.Failed, vendResponse.Response + " " + vendResponse.Message, transactionId, vendResponse.Token, vendResponse.keyChangeToken, vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif).ConfigureAwait(false);
        //                            validationMessage = "Vend request failed, amount credited to wallet.";
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                validationMessage = "User not found.";
        //            }
        //        }

        //        response.Add(new BankNotificationResponseDto
        //        {
        //            NetUpTransactionGuid = request.NetUpTransactionGuid,
        //            Message = validationMessage,
        //            Status = 500
        //        });
        //        var addBankTransferTransaction = new AddBankTransferTransactionDto
        //        {
        //            Amount = request.TransactionValue,
        //            TransactionId = strTransactionNumber,
        //            Flag = (int)PaymentStatus.Failed,
        //            PaymentMethodId = paymentMethodId.Id,
        //            PfResponse = JsonConvert.SerializeObject(request),
        //            RechargeAmount = request.TransactionValue,
        //            TopupStatus = TopUpStatusEnum.DebitechValidationFailed.ToString(),
        //            NetUpTransactionGuid = request.NetUpTransactionGuid,
        //            UseWallet = 0,
        //            Remark = validationMessage,
        //            BankTransactionId = request.BankTransactionId
        //        };
        //        _ = await _topUpRepository.AddBankTransferTransaction(addBankTransferTransaction).ConfigureAwait(false);
        //    }
        //    return response;
        //}

        public async Task<IEnumerable<BankNotificationResponseDto>> Handle(BankNotificationRequestQuery bankRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In notify handler");
            int debitechNotifyId = await _topUpRepository.SaveDebitechNotifyRequest(bankRequest).ConfigureAwait(false);

            var getPaymentMethodsQuery = new GetPaymentMethodsQuery()
            {
                StatusId = 0
            };
            var paymentMethodIds = await _topUpRepository.GetDebitechPaymentMethods(getPaymentMethodsQuery).ConfigureAwait(false);
            var paymentMethodId = paymentMethodIds
                                    .Where(t => t.Slug.Contains("bank-transfer", StringComparison.CurrentCultureIgnoreCase))
                                    .FirstOrDefault();
            var response = new List<BankNotificationResponseDto>();
            double amountForFeeCalucation = 0.0;
            try
            {
                foreach (var request in bankRequest.BankNotificationRequest)
                {
                    string validationMessage = "";
                    var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
                    var transactionReferenceNumber = Guid.NewGuid().ToString();
                    request.TrimAllStrings();
                    bool isValid = true;
                    var msgRes = new PushNotificationDto();
                    //bool isGuidIdValid = Guid.TryParse(request.NetUpTransactionGuid, out Guid guidOutput);
                    //if (!isGuidIdValid)
                    //{
                    //    validationMessage = "NetUpTransactionGuid is invalid Guid.";
                    //    isValid = false;
                    //}
                    var count = await _topUpRepository.IsNetUpTransactionGuidExist(request.NetUpTransactionGuid).ConfigureAwait(false);
                    if (count > 0)
                    {
                        validationMessage = "NetUpTransactionGuid already exist.";
                        isValid = false;
                    }
                    count = await _topUpRepository.IsBankTransactionIdExist(request.BankTransactionId).ConfigureAwait(false);
                    if (count > 0)
                    {
                        validationMessage = "BankTransactionId already exist.";
                        isValid = false;
                    }

                    var validRequest = _bankNotificationValidator.Validate(request);
                    if (!validRequest)
                    {
                        validationMessage = "NetUpChecksum validation failed.";
                        isValid = false;
                    }

                    if (isValid)
                    {
                        var user = new UserDto();
                        request.PayerReferenceNumber = request.PayerReferenceNumber.ToLower();
                        string serialNumber = request.PayerReferenceNumber.Substring(2, 2);

                        string strPrecharacterEft = "";
                        var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                        if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("eftprecharacters")))
                        {

                            var approvalConfig = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("eftprecharacters"));
                            if (approvalConfig != null)
                            {
                                strPrecharacterEft = approvalConfig.Value;
                            }
                        }
                        serialNumber = request.PayerReferenceNumber.Substring(2, 2);
                        if (serialNumber == "00")
                            user = await _userRepository.GetUserByPayerReferenceNumber(request.PayerReferenceNumber);

                        else
                        {
                            string newEFT = "";

                            char[] chars = request.PayerReferenceNumber.ToCharArray(); // Convert to char array
                            chars[3] = '0'; // Replace 4th character (index 3)
                            newEFT = new string(chars);

                            int propertyId = await _meterRepository.GetPropertyIdByEFTNumber(newEFT).ConfigureAwait(false);
                            int meterId = await _meterRepository.GetMeterIdByEFTNumber(newEFT).ConfigureAwait(false);
                            int userId = await _propertyUserRepository.getUserIdBySerialNumberPropertyId(Convert.ToInt32(serialNumber), propertyId).ConfigureAwait(false);
                            user = await _userRepository.GetPropertyUserById(userId, propertyId).ConfigureAwait(false);

                        }

                        if (user != null)
                        {
                            var walletBalance = await _walletRepository.GetBalanceAsync(user.Id);
                            TransactionFeeDto feeDto = new TransactionFeeDto();



                            var bankTransferTransaction = new AddBankTransferTransactionDto
                            {
                                Amount = request.TransactionValue,
                                TransactionId = strTransactionNumber,
                                Flag = (int)PaymentStatus.Complete,
                                PaymentMethodId = paymentMethodId.Id,
                                PfResponse = JsonConvert.SerializeObject(request),
                                RechargeAmount = request.TransactionValue - feeDto.TransactionFee,
                                TransactionFee = feeDto.TransactionFee,
                                UserId = user.Id,
                                MeterId = user.MeterId,
                                TopupStatus = TopUpStatusEnum.DebitechSuccess.ToString(),
                                UseWallet = 0,
                                NetUpTransactionGuid = request.NetUpTransactionGuid,
                                BankTransactionId = request.BankTransactionId,
                                EFTReferenceNumber = request.PayerReferenceNumber,

                            };
                            var transactionId = await _topUpRepository.AddBankTransferTransaction(bankTransferTransaction).ConfigureAwait(false);

                            if (transactionId > 0)
                            {
                                var payFastModel = new PayFastModel
                                {
                                    merchant_id = "Debitech request",
                                    m_payment_id = strTransactionNumber,
                                    payment_status = "COMPLETE",
                                    pf_payment_id = request.BankTransactionId,
                                    signature = request.NetUpChecksum
                                };
                                var userWallet = new UserWalletDto();
                                var walletAmountToUse = 0.0;
                                var result = await _topUpRepository.UpdateTopupTransactions(payFastModel).ConfigureAwait(false);
                                var topupTransaction = await _topUpRepository.GetTopupTransactionDetails(payFastModel.m_payment_id);
                                if (topupTransaction != null)
                                {
                                    string transactionRemark = "";
                                    var company = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
                                    var userDetails = await _userRepository.GetUserById(topupTransaction.UserId).ConfigureAwait(true);
                                    int walletId = await _walletRepository.IsWalletExist(topupTransaction.UserId).ConfigureAwait(false);
                                    if (walletId > 0)
                                    {
                                        userWallet = await _walletRepository.GetUserWalletById(topupTransaction.UserId);


                                        UpdateWalletBalanceQuery updateWalletBalance = new UpdateWalletBalanceQuery
                                        {
                                            UserId = user.Id,
                                            Amount = topupTransaction.Amount
                                        };
                                        //var userWallet = await _walletRepository.GetUserWalletById(updateWalletBalance.UserId);

                                        //var walletAmountToUse = userWallet.Balance;
                                        //walletAmountToUse = topupTransaction.Amount >= walletAmountToUse ? walletAmountToUse : topupTransaction.Amount;
                                        //// double UpdatedBalance = userWallet.Balance - walletAmountToUse;
                                        //transactionRemark = "Debited after debitech transaction";
                                        //await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, walletAmountToUse, userWallet.Balance, transactionRemark, false).ConfigureAwait(false);


                                        walletAmountToUse = userWallet.Balance;
                                        if (userWallet.Balance > 0)
                                        {
                                            walletAmountToUse = topupTransaction.Amount >= walletAmountToUse ? walletAmountToUse : topupTransaction.Amount;
                                            // double UpdatedBalance = userWallet.Balance - walletAmountToUse;

                                            if (topupTransaction.Amount >= userWallet.Balance)
                                            {

                                                amountForFeeCalucation = topupTransaction.Amount - walletAmountToUse;
                                            }
                                            if (topupTransaction.Amount <= userWallet.Balance)
                                            {
                                                amountForFeeCalucation = 0;
                                            }

                                            transactionRemark = "Debited after Topup transaction";
                                            await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, walletAmountToUse, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.TransactionID).ConfigureAwait(false);
                                            feeDto = await _transactionFeesService.CalculateTransactionFee(paymentMethodId.Id, amountForFeeCalucation, false, user.Id);
                                            result = await _topUpRepository.UpdateTopupTransactionFee(walletAmountToUse + feeDto.TopUpAmount, feeDto.TransactionFee, topupTransaction.TransactionID).ConfigureAwait(false);

                                        }
                                        else
                                        {
                                            feeDto = await _transactionFeesService.CalculateTransactionFee(paymentMethodId.Id, topupTransaction.Amount, false, user.Id);
                                            result = await _topUpRepository.UpdateTopupTransactionFee(walletAmountToUse + Math.Round(feeDto.TopUpAmount, 4), feeDto.TransactionFee, topupTransaction.TransactionID).ConfigureAwait(false);
                                        }
                                    }
                                    else
                                    {
                                        feeDto = await _transactionFeesService.CalculateTransactionFee(paymentMethodId.Id, topupTransaction.Amount, false, user.Id);
                                        result = await _topUpRepository.UpdateTopupTransactionFee(walletAmountToUse + Math.Round(feeDto.TopUpAmount, 4), feeDto.TransactionFee, topupTransaction.TransactionID).ConfigureAwait(false);
                                    }
                                    string mailBody = "";
                                    string vendPayMethod = "";
                                    if (paymentMethodId != null && paymentMethodId.Id > 0)
                                    {
                                        vendPayMethod = await _topUpRepository.GetIPayMethodByPaymentMethodId(paymentMethodId.Id).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        vendPayMethod = "other";
                                    }
                                    _logger.LogInformation("Before vend in  notify handler");
                                    var vendResponse = await _vendRequestHelper.ProcessRecharge(topupTransaction, vendPayMethod).ConfigureAwait(true);
                                    if (vendResponse.StatusCode == 200)
                                    {
                                        _logger.LogInformation("after vend succeess in  notify handler");
                                        await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendSuccess.ToString(), (int)PaymentStatus.Complete, vendResponse.Response, transactionId, vendResponse.Token, vendResponse.keyChangeToken, vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif, vendResponse.VendReference).ConfigureAwait(false);
                                        var sendReceiptToEmail = new DownloadPurchaceRecieptPdfQuery
                                        {
                                            TransactionId = topupTransaction.TransactionID
                                        };

                                        // toto: call push notification

                                        string deviceToken = await _userRepository.GetDeviceToken(user.Id).ConfigureAwait(false);
                                        var sendMessage = new DownloadPurchaceRecieptPdfQuery
                                        {
                                            TransactionId = topupTransaction.TransactionID
                                        };
                                        var PurchaseDetails = await _topUpRepository.GetReceiptDetails(sendMessage).ConfigureAwait(false);
                                        if (!string.IsNullOrEmpty(deviceToken) && deviceToken != "string")
                                        {

                                            string title = "Top Up";
                                            string body = "";
                                            if (PurchaseDetails != null)
                                            {
                                                if (!string.IsNullOrEmpty(PurchaseDetails.StandardTokens))
                                                {

                                                    var amount = PurchaseDetails.stdamt.ToString().Replace(",", ".");
                                                    mailBody = company.Name +
                                                      "<br> Thank you for your payment. The transaction details are as follows:" +
                                                     "<br>Payment received." +
                                                     "<br>Meter: " + PurchaseDetails.MeterNumber +
                                                     "<br>RCT: " + PurchaseDetails.RCTNo +
                                                     "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                     "<br>Token: " + PurchaseDetails.StandardTokens +
                                                     "<br>Units: " + PurchaseDetails.Units +
                                                     "<br>: Please review the attached purchase receipt for further information.";
                                                    body = mailBody;
                                                }
                                                if (!string.IsNullOrEmpty(PurchaseDetails.BsstToken))
                                                {
                                                    var amount = PurchaseDetails.BsstTokenAmount.ToString().Replace(",", ".");
                                                    mailBody = company.Name +
                                                        "<br> Thank you for your payment. The transaction details are as follows:" +
                                                      "<br>Payment received." +
                                                      "<br>RCT: " + PurchaseDetails.RCTNo +
                                                      "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                      "<br>: Please review the attached purchase receipt for further information.";
                                                    body = mailBody;
                                                }
                                                else
                                                {
                                                    var amount = PurchaseDetails.ActualRechargeAmount.ToString().Replace(",", ".");
                                                    mailBody = company.Name +
                                                        "<br> Thank you for your payment. The transaction details are as follows:" +
                                                      "<br>Payment received." +
                                                      "<br>RCT: " + PurchaseDetails.RCTNo +
                                                      "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                      "<br>Remaining bal: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                       "<br>: Please review the attached purchase receipt for further information.";
                                                }

                                                body = mailBody;
                                                msgRes = await _pushNotification.SendMessage(title, body, deviceToken, topupTransaction.UserId).ConfigureAwait(false);
                                            }
                                            if (msgRes.ResponseMsg == "Messgae sent successfully")
                                            {
                                                AddOrUpdateNotificationsQuery newNotification = new()
                                                {
                                                    UserID = topupTransaction.UserId,
                                                    Title = "Top Up successfull",
                                                    Description = "Top up successfull, recharged with amount: R" + topupTransaction.Amount + " .",
                                                    IsRead = (int)StatusEnum.Sent,
                                                    NotificationType = (int)NotificationType.Updated

                                                };
                                                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                                            }
                                        }
                                        _ = await _topUpRepository.DownloadPurchaceRecieptPdfQuery(sendReceiptToEmail, user.Id).ConfigureAwait(false);


                                        response.Add(new BankNotificationResponseDto
                                        {
                                            NetUpTransactionGuid = request.NetUpTransactionGuid,
                                            Message = "Transaction updated successfully.",
                                            Status = 200
                                        });
                                        continue;
                                    }
                                    else
                                    {
                                        //feeDto = await _transactionFeesService.CalculateTransactionFee(paymentMethodId.Id, topupTransaction.Amount, false, user.Id);
                                        //result = await _topUpRepository.UpdateTopupTransactionFee(walletAmountToUse + Math.Round(feeDto.TopUpAmount, 4), feeDto.TransactionFee, topupTransaction.TransactionID).ConfigureAwait(false);
                                        _logger.LogInformation("after vend failed in  notify handler");
                                        transactionRemark = "Recharge amount credited after debitech failed transaction";
                                        var vednFailedMessage = vendResponse.Message;
                                        string title = "Top Up";
                                        string body = "";
                                        await _walletRepository.AddBalanceAsync(topupTransaction.UserId, Math.Round(feeDto.TopUpAmount, 4), userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount, topupTransaction.TransactionID).ConfigureAwait(false);
                                        AddOrUpdateNotificationsQuery newNotification = new()
                                        {
                                            UserID = topupTransaction.UserId,
                                            Title = "Top Up Failed",
                                            Description = "Debitech Top up failed. Amount : R " + feeDto.TopUpAmount + " credited to wallet balance.",
                                            IsRead = (int)StatusEnum.Sent,
                                            NotificationType = (int)NotificationType.Updated

                                        };
                                        await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

                                        mailBody = company.Name +
                                                       "<br>Transaction failed reason: " + vednFailedMessage +
                                                       "<br>Transaction remark: " + transactionRemark +
                                                       "<br>Amt: R " + topupTransaction.Amount +
                                                       "<br>Transaction fee: R " + topupTransaction.TransactionFee +
                                                       "<br>Recharge amount: R " + feeDto.TopUpAmount;


                                        body = mailBody;


                                        await SendEmailMessage(topupTransaction, userDetails, body).ConfigureAwait(false);

                                        await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendFailed.ToString(), (int)PaymentStatus.Failed, vendResponse.Response, transactionId, vendResponse.Token, vendResponse.keyChangeToken, vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif, vendResponse.VendReference).ConfigureAwait(false);
                                        validationMessage = "Vend request failed, amount credited to wallet.";
                                    }
                                }
                            }
                        }
                        else
                        {

                            var bankFailedTransferTransaction = new AddDeitecFailedTransactionDto
                            {
                                Amount = request.TransactionValue,
                                TransactionId = strTransactionNumber,
                                ReferenceNo = request.PayerReferenceNumber
                            };
                            var transactionId = await _topUpRepository.AddDebitechFailedTxn(bankFailedTransferTransaction).ConfigureAwait(false);
                            _logger.LogInformation("In notify failed transaction");
                            validationMessage = "User not found.";


                        }
                    }
                    if (!isValid)
                    {
                        validationMessage = "Transaction is already processed.";
                        response.Add(new BankNotificationResponseDto
                        {
                            BankTransactionId = request.BankTransactionId,
                            NetUpTransactionGuid = request.NetUpTransactionGuid,
                            Message = validationMessage,
                            Status = 200
                        });

                        await _topUpRepository.DeleteDebitechDuplicateNotifyRequest(debitechNotifyId).ConfigureAwait(false);
                    }
                    else
                    {
                        response.Add(new BankNotificationResponseDto
                        {
                            BankTransactionId = request.BankTransactionId,
                            NetUpTransactionGuid = request.NetUpTransactionGuid,
                            Message = validationMessage,
                            Status = 500
                        });
                    }

                   
                    //var addBankTransferTransaction = new AddBankTransferTransactionDto
                    //{
                    //    Amount = request.TransactionValue,
                    //    TransactionId = strTransactionNumber,
                    //    Flag = (int)PaymentStatus.Failed,
                    //    PaymentMethodId = paymentMethodId.Id,
                    //    PfResponse = JsonConvert.SerializeObject(request),
                    //    RechargeAmount = request.TransactionValue,
                    //    TopupStatus = TopUpStatusEnum.DebitechValidationFailed.ToString(),
                    //    NetUpTransactionGuid = request.NetUpTransactionGuid,
                    //    UseWallet = 0,
                    //    Remark = validationMessage,
                    //    BankTransactionId = request.BankTransactionId
                    //};
                    //_ = await _topUpRepository.AddBankTransferTransaction(addBankTransferTransaction).ConfigureAwait(false);
                }
                debitechNotifyId = await _topUpRepository.UpdateDebitechNotifyResponse(response, debitechNotifyId).ConfigureAwait(false);
                _logger.LogInformation("after response saved in notify handler");
            }
            catch (Exception ex)
            {
                _logger.LogError("In debitech handler catch : " + ex.ToString());
            }
            return response;
        }

        public async Task SendEmailMessage(GetTopUpTransaction topupTransaction, UserProfileDto user, string body)
        {
            var communications = await _communicationRepository.GetCommunicationsByUserId(topupTransaction.UserId).ConfigureAwait(false);
            foreach (var communication in communications)
            {
                var name = communication.Name?.ToLower(); // To handle case-insensitive check
                if (user.Mobile.Length == 10)
                {
                    user.Mobile = user.Mobile.TrimStart('0');
                }
                if (name != null && name.Contains("mobile"))
                {

                    await _otpService.SendMobileOtp("", body, user.CountryCode + user.Mobile);

                }
                if (name != null && name.Contains("email"))
                {
                    EmailModelClass obj = new()
                    {
                        title = "Top Up Failed",
                        email = user.Email,
                        forEvent = "TopUpFailed",
                        subtitle = "",
                        companyId = user.CompanyId,
                        mobile = user.Mobile,
                        propertyUser = user.UserName,
                        body = body,
                        documentPath = ""
                    };
                    await _otpService.SendEventMail(obj).ConfigureAwait(false);
                }
            }
        }
    }
}
