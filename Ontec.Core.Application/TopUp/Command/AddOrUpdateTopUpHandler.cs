using System.Text;
using iTextSharp.text.log;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Models.Dto.VendRequest;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.TopUp.Command;
using Ontec.Core.Domain.Requests.TopUp.Queries;

namespace Ontec.Core.Application.TopUp.Command
{
    public class AddOrUpdateTopUpHandler : IRequestHandler<AddOrUpdateBankAccountQuery, AddUpdateResultDto>
                                              , IRequestHandler<DeleteBankAccountbyId, string>
                                            , IRequestHandler<AddTopUpTransactionQuery, AddTopUpTransactionsDto>
                                           , IRequestHandler<PayFastModel, VendRequestResponse>
                                            , IRequestHandler<UpdatePaymentMethodsQuery, string>
                                            , IRequestHandler<CancelTopUpTransactionQuery, CancelTransactionResponseModel>
                                            , IRequestHandler<WalletTopUpModel, VendRequestResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpService _otpService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITopUpRepository _topUpRepository;
        private readonly IWorkContext _workContext;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ITransactionFeesService _transactionFeesService;
        private readonly IWalletRepository _walletRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IVendRequestHelper _vendRequestHelper;
        private readonly IPushNotification _pushNotification;
        private readonly ICompanyHelper _companyHelper;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly ILogger<AddOrUpdateTopUpHandler> _logger;
        private readonly IDocumentRepository _documentRepository;
        public AddOrUpdateTopUpHandler(IWorkContext workContext, IUserRepository userRepository,
                                          ITopUpRepository topUpRepository, IEncryptionandDecryption encryptionandDecryption
                                           , IPropertyRepository propertyRepository
                                           , ITransactionFeesService transactionFeesService
                                            , IWalletRepository walletRepository
                                            , IConfigurationRepository configurationRepository
                                            , IMeterRepository meterRepository
                                            , IVendRequestHelper vendRequestHelper
                                            , IPushNotification pushNotification
                                            , ICompanyHelper companyHelper
                                          , INotificationRepository notificationRepository
                                            , ICommunicationRepository communicationRepository
                                            , IOtpService otpService
                                          , IDocumentRepository documentRepository
            , ILogger<AddOrUpdateTopUpHandler> logger)
        {
            _workContext = workContext;
            _userRepository = userRepository;
            _topUpRepository = topUpRepository;
            _propertyRepository = propertyRepository;
            _transactionFeesService = transactionFeesService;
            _walletRepository = walletRepository;
            _configurationRepository = configurationRepository;
            _meterRepository = meterRepository;
            _vendRequestHelper = vendRequestHelper;
            _pushNotification = pushNotification;
            _companyHelper = companyHelper;
            _notificationRepository = notificationRepository;
            _communicationRepository = communicationRepository;
            _otpService = otpService;
            _logger = logger;
            _documentRepository = documentRepository;
        }

        #region BankAccount
        public async Task<AddUpdateResultDto> Handle(AddOrUpdateBankAccountQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new AddOrUpdateBankAccountValidator(_topUpRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;
            if (request.Id > 0)
            {
                result = await _topUpRepository.UpdateBankAccount(request).ConfigureAwait(false);
            }
            else
            {
                result = await _topUpRepository.AddBankAccount(request).ConfigureAwait(false);
            }
            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Bank account added successfully!";
                else
                    response.Message = "Bank account updated successfully!";
            }
            return response;
        }

        public async Task<string> Handle(DeleteBankAccountbyId request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new DeleteBankAccountbyIdValidator(_topUpRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            await _topUpRepository.DeleteBankAccountById(request.Id).ConfigureAwait(false);

            return "Deleted successfully!";
        }
        #endregion

        public async Task<AddTopUpTransactionsDto> Handle(AddTopUpTransactionQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new AddTopUpTransactionQueryValidaotr(_userRepository, _workContext, _meterRepository, _configurationRepository,_topUpRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            bool isLekkaPay = false;
            string currentPaymentGateWay = "";
            int result = 0;
            TransactionFeeDto feeDto = new TransactionFeeDto();
            var debts = new List<DebtItem>();
            var fixedItems = new List<FixedItem>();
            decimal totalDebtAmount = 0;
            decimal totalFixedAmunt = 0;
            decimal totalVatExcluding = 0;
            decimal totalVatIncluding = 0;
            decimal totalTax = 0;
            AddTopUpTransactionsDto transactionsDto = new AddTopUpTransactionsDto();

            var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
            var userWallet = await _walletRepository.GetUserWalletById(request.UserId);
            if (request.UseWallet && request.PaymentMethodId == 0 && userWallet.Balance > request.Amount)
            {
                request.PaymentMethodId = (int)PaymentMethodsEnum.Wallet;
            }
            var editableConfiguration = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
            if (editableConfiguration != null)
            {
                var paymentGateway = editableConfiguration.Where(t => t.Name.Contains("islekkapay", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (paymentGateway != null)
                {
                    string paymentGatewayValue = paymentGateway.Value;
                    if (paymentGatewayValue == "1")
                    {
                        isLekkaPay = true;
                        currentPaymentGateWay = "LekkaPay";
                    }
                    else
                    {
                        isLekkaPay = false;
                        currentPaymentGateWay = "PayFast";
                    }
                }
            }
            feeDto = await _transactionFeesService.CalculateTransactionFee(request.PaymentMethodId, request.Amount, request.UseWallet, request.UserId);
            result = await _topUpRepository.AddTopupTransactions(request, strTransactionNumber, feeDto.TransactionFee, feeDto.TopUpAmount, feeDto.WalletAmountUsed, feeDto.FinalAmountToPay, currentPaymentGateWay).ConfigureAwait(false);
            if (result > 0)
            {

                var vendRequest = new GetTopUpTransaction
                {
                    RechargeAmount = feeDto.TopUpAmount,
                    MeterId = request.MeterId
                };
                string vendPayMethod = "";
                if (request.UseWallet && request.PaymentMethodId > 0 && request.PaymentMethodId != (int)PaymentMethodsEnum.Wallet)
                {
                    vendPayMethod = await _topUpRepository.GetIPayMethodByPaymentMethodId(request.PaymentMethodId).ConfigureAwait(false);
                }
                if (!request.UseWallet && request.PaymentMethodId > 0 && request.PaymentMethodId != (int)PaymentMethodsEnum.Wallet)
                {
                    vendPayMethod = await _topUpRepository.GetIPayMethodByPaymentMethodId(request.PaymentMethodId).ConfigureAwait(false);
                }
                else
                {
                    vendPayMethod = "none";

                }
                var trailVendResponse = await _vendRequestHelper.ProcessVendTrail(vendRequest, vendPayMethod).ConfigureAwait(true);
                if (trailVendResponse.StatusCode == 200)
                {
                    await _topUpRepository.UpdateTrailTransactionStatus(TopUpStatusEnum.TrailVendSuccess.ToString(), (int)PaymentStatus.Complete, trailVendResponse.Response, result, trailVendResponse.Debt).ConfigureAwait(false);
                    response.Message = "Transaction saved successfully";
                    transactionsDto = await _topUpRepository.GetTopUpTransaction(result);
                    if (transactionsDto.TrailVendResponseJson != null)
                    {
                        JObject jsonObject = JObject.Parse(transactionsDto.TrailVendResponseJson);

                        bool containsDebt = jsonObject["ipayMsg"]?["elecMsg"]?["trialVendRes"]?["debt"] != null;
                        bool containsFixed = jsonObject["ipayMsg"]?["elecMsg"]?["trialVendRes"]?["fixed"] != null;
                        if (containsDebt)
                        {
                            decimal debtAmount = 0;
                            decimal debtTax = 0;
                            int debtCount = (Int32)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"].Count();

                            if (debtCount != 8)
                            {
                                JArray debtArray = (JArray)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"];
                                if (debtArray != null && debtArray.Count > 0)
                                {

                                    foreach (var debt in debtArray)
                                    {

                                        if (debt != null)
                                        {
                                            string text = (string)debt["#text"];
                                            string amount = (string)debt["@amt"];
                                            string remainingBalance = (string)debt["@rem"];
                                            string tax = (string)debt["@tax"];
                                            string accountType = (string)debt["@accountType"];
                                            debtAmount = Convert.ToDecimal(amount);
                                            debtAmount = Math.Round(debtAmount / 100, 2);
                                            debtTax = Convert.ToDecimal(tax);
                                            debtTax = Math.Round(debtTax / 100, 2);
                                            decimal debtremainingBalance = Convert.ToDecimal(remainingBalance);
                                            debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);
                                            if (accountType == "debt")
                                            {
                                                debts.Add(new DebtItem
                                                {
                                                    Amount = debtAmount,
                                                    Tax = debtTax,
                                                    RemainBalance = debtremainingBalance,
                                                    Text = text
                                                });
                                                totalDebtAmount += (debtAmount) + (debtTax);
                                                totalTax += debtTax;
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                decimal amount = 0;
                                decimal tax = 0;
                                decimal remainingBalance = 0;
                                amount = (decimal)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"]["@amt"];
                                tax = (decimal)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"]["@tax"];
                                string text = (string)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"]["#text"];
                                remainingBalance = (decimal)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"]["@rem"];
                                debtAmount = Convert.ToDecimal(amount);
                                debtAmount = Math.Round(debtAmount / 100, 2);
                                debtTax = Convert.ToDecimal(tax);
                                debtTax = Math.Round(debtTax / 100, 2);
                                decimal debtremainingBalance = Convert.ToDecimal(remainingBalance);
                                debtremainingBalance = Math.Round(debtremainingBalance / 100, 2);

                                debts.Add(new DebtItem
                                {
                                    Amount = debtAmount,
                                    Tax = debtTax,
                                    RemainBalance = debtremainingBalance,
                                    Text = text

                                });
                                totalDebtAmount += (debtAmount) + (debtTax);
                                totalTax += debtTax;
                            }
                        }
                        if (containsFixed)
                        {
                            int fixedCount = (Int32)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["fixed"].Count();
                            decimal fixedAmount = 0;
                            decimal fixedTax = 0;
                            if (fixedCount != 5)
                            {
                                JArray fixedArray = (JArray)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["fixed"];
                                if (fixedArray != null && fixedArray.Count > 0)
                                {
                                    foreach (var fix in fixedArray)
                                    {

                                        if (fix != null)
                                        {
                                            string text = (string)fix["#text"];
                                            string amount = (string)fix["@amt"];
                                            string tax = (string)fix["@tax"];
                                            fixedAmount = Convert.ToDecimal(amount);
                                            fixedAmount = Math.Round(fixedAmount / 100, 2);
                                            fixedTax = Convert.ToDecimal(tax);
                                            fixedTax = Math.Round(fixedTax / 100, 2);
                                            fixedItems.Add(new FixedItem
                                            {
                                                Tax = fixedTax,
                                                Amount = fixedAmount,
                                                Text = text

                                            });
                                            totalFixedAmunt += (fixedAmount);
                                            totalTax += fixedTax;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                double amount = (double)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["fixed"]["@amt"];
                                double tax = (double)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["fixed"]["@tax"];
                                string text = (string)jsonObject["ipayMsg"]["elecMsg"]["trialVendRes"]["fixed"]["#text"];
                                amount = Math.Round(amount / 100, 2);
                                tax = Math.Round(tax / 100, 2);
                                fixedAmount = Convert.ToDecimal(amount);
                                fixedTax = Convert.ToDecimal(tax);
                                fixedItems.Add(new FixedItem
                                {
                                    Tax = fixedTax,
                                    Amount = fixedAmount,
                                    Text = text

                                });
                                totalFixedAmunt += (fixedAmount);
                                totalTax += fixedTax;
                            }

                        }

                    }
                }
                else
                {
                    await _topUpRepository.UpdateTrailTransactionStatus(TopUpStatusEnum.TrailVendFailed.ToString(), (int)PaymentStatus.Failed, trailVendResponse.Response + " " + trailVendResponse.Message, result, trailVendResponse.Debt).ConfigureAwait(false);
                }
                transactionsDto.TrailVendResponse = trailVendResponse;
            }
            return transactionsDto;

        }

        //Notify payfast response when wallet is not selected
        public async Task<VendRequestResponse> Handle(PayFastModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Entered in PayFastModel handler");
            var response = new VendRequestResponse();
            try
            {
                request.TrimAllStrings();
                var commonValidator = new UpdateNotifyTopUpTransactionQueryValidator(_topUpRepository);
                var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);

                int result = 0;

                var topupTransaction = await _topUpRepository.GetTopupTransactionDetails(request.m_payment_id);
                var user = await _userRepository.GetUserById(topupTransaction.UserId).ConfigureAwait(true);
                var companyDetails = new CompanyDetailsDto();
                try
                {
                    //int id = await _topUpRepository.IsTransactionNoExist(request.m_payment_id).ConfigureAwait(false);

                    companyDetails = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, user.CompanyId.ToString());
                    throw;
                }
               
                if (string.IsNullOrEmpty(topupTransaction.PayFastResponse) && !string.IsNullOrEmpty(topupTransaction.TopupStatus) && topupTransaction.TopupStatus == "TrailVendSuccess" && string.IsNullOrEmpty(topupTransaction.VendResponse) && string.IsNullOrEmpty(topupTransaction.ReceiptNumber))
                {
                    _logger.LogInformation("Entered in if Condition");
                    _logger.LogInformation("validated txn not processed previouslvalidated txn not processed previousl");
                    LogToFile(companyDetails.WWWPath, "validated txn not processed previously");
                    LogToFile(companyDetails.WWWPath, request.pf_payment_id + " " + request.signature + " " + request.merchant_id + " " + request.payment_status);
                    _logger.LogInformation("pf_payment_id :" + request.pf_payment_id + " ," + " Signature : "+ request.signature + " ," + " merchant_id : "+ request.merchant_id + " , " + "payamnet_status : " + request.payment_status);


                    _logger.LogInformation("before update payfast response ");
                    try
                    {
                        result = await _topUpRepository.UpdateTopupTransactions(request).ConfigureAwait(false);
                        //LogToFile(companyDetails.WWWPath, $"UpdateTopupTransactions returned: {result}");

                        _logger.LogInformation("UpdateTopupTransactions returned: " + result);
                    }
                    catch (Exception ex)
                    {
                        LogToFile(companyDetails.WWWPath, $"Exception in UpdateTopupTransactions: {ex.Message} {ex.StackTrace}");
                        _logger.LogInformation("Exception in UpdateTopupTransactions " + ex.Message);
                        throw;
                    }
                    try
                    {
                        _logger.LogInformation("before UpdateTransactionStatus");
                        await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.PayFastSuccess.ToString(), (int)PaymentStatus.Failed, null, topupTransaction.Id, null, null, null, null, null, null, null, null).ConfigureAwait(false);
                        _logger.LogInformation("after  UpdateTransactionStatus");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Exception in UpdateTransactionStatus " + ex.Message);
                        throw;
                    }
                    if (result > 0)
                    {
                        _logger.LogInformation("if result >0  " + result);
                        response.StatusCode = 200;
                        string transactionRemark = "";
                        //var topupTransaction = await _topUpRepository.GetTopupTransactionDetails(request.m_payment_id);

                        if (topupTransaction != null)
                        {

                            var company = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
                            var userWallet = await _walletRepository.GetUserWalletById(topupTransaction.UserId);
                            var walletAmountToUse = userWallet.Balance;
                            //if (userWallet.Balance > 0)
                            //{
                            //    if (topupTransaction.UseWallet)
                            //    {
                            //        walletAmountToUse = topupTransaction.Amount >= walletAmountToUse ? walletAmountToUse : topupTransaction.Amount;
                            //        // double UpdatedBalance = userWallet.Balance - walletAmountToUse;
                            //        transactionRemark = "Debited after Topup transaction";
                            //        await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, walletAmountToUse, userWallet.Balance, transactionRemark, topupTransaction.UseWallet,topupTransaction.TransactionID).ConfigureAwait(false);
                            //    }
                            //}
                            //ToDo: Add entry to wallet transation table
                            //
                            string vendPayMethod = "";
                            if (topupTransaction.UseWallet && topupTransaction.PaymentMethodId > 0 && topupTransaction.PaymentMethodId != (int)PaymentMethodsEnum.Wallet)
                            {
                                vendPayMethod = await _topUpRepository.GetIPayMethodByPaymentMethodId(topupTransaction.PaymentMethodId).ConfigureAwait(false);
                            }
                            if (!topupTransaction.UseWallet && topupTransaction.PaymentMethodId > 0 && topupTransaction.PaymentMethodId != (int)PaymentMethodsEnum.Wallet)
                            {
                                vendPayMethod = await _topUpRepository.GetIPayMethodByPaymentMethodId(topupTransaction.PaymentMethodId).ConfigureAwait(false);
                            }
                            else
                            {
                                vendPayMethod = "none";
                            }
                            LogToFile(companyDetails.WWWPath, "before process recharge ");
                            var vendResponse = await _vendRequestHelper.ProcessRecharge(topupTransaction, vendPayMethod).ConfigureAwait(true);
                            LogToFile(companyDetails.WWWPath, "after process recharge");

                            response = vendResponse;
                            string deviceToken = await _userRepository.GetDeviceToken(topupTransaction.UserId).ConfigureAwait(false);
                            string mailBody = "";
                            if (vendResponse.StatusCode == 200)
                            {

                                await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendSuccess.ToString(), (int)PaymentStatus.Complete, vendResponse.Response, topupTransaction.Id, vendResponse.Token, vendResponse.keyChangeToken,
                                                                                vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif, vendResponse.VendReference).ConfigureAwait(false);


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
                                                    "<br> Payment received." +
                                                    "<br>Meter: " + PurchaseDetails.MeterNumber +
                                                    "<br>RCT: " + PurchaseDetails.ReceiptId +
                                                    "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                    "<br>Token: " + PurchaseDetails.StandardTokens +
                                                    "<br>Units: " + PurchaseDetails.Units;
                                            body = mailBody;
                                        }
                                        if (!string.IsNullOrEmpty(PurchaseDetails.BsstToken))
                                        {
                                            var amount = PurchaseDetails.BsstTokenAmount.ToString().Replace(",", ".");
                                            mailBody = company.Name +
                                                     "<br> Payment received." +
                                                     "<br>RCT: " + PurchaseDetails.ReceiptId +
                                                     "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2);
                                            body = mailBody;
                                        }
                                        else
                                        {
                                            var amount = PurchaseDetails.ActualRechargeAmount.ToString().Replace(",", ".");
                                            mailBody = company.Name +
                                                      "<br>Payment received." +
                                                      "<br>RCT: " + PurchaseDetails.RctNo +
                                                      "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                      "<br>Remaining bal: R " + Math.Round(Convert.ToDecimal(userWallet.Balance) / 100, 2);
                                            body = mailBody;


                                        }

                                        await _pushNotification.SendMessage(title, body, deviceToken, topupTransaction.UserId).ConfigureAwait(false);



                                    }
                                    var mailReq = new DownloadPurchaceRecieptPdfQuery()
                                    {
                                        TransactionId = topupTransaction.TransactionID,

                                    };
                                    await _topUpRepository.DownloadPurchaceRecieptPdfQuery(mailReq, topupTransaction.UserId).ConfigureAwait(false);


                                    AddOrUpdateNotificationsQuery newNotification = new()
                                    {
                                        UserID = topupTransaction.UserId,
                                        Title = "Top Up successfull",
                                        Description = "Top up successfull, recharged with amount: R " + topupTransaction.Amount + " .",
                                        IsRead = (int)StatusEnum.Sent,
                                        NotificationType = (int)NotificationType.Updated

                                    };
                                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                                }

                                transactionRemark = "Debited after Topup Successfull Transaction";
                                if (topupTransaction.UseWallet && userWallet.Balance > 0 && userWallet.Balance <= topupTransaction.Amount)
                                {
                                    await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, userWallet.Balance, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.TransactionID).ConfigureAwait(false);

                                    //await _walletRepository.saveTransaction((int)TransactionType.Debit, userWallet.Balance, transactionRemark, userWallet.Id);
                                }
                                if (topupTransaction.UseWallet && userWallet.Balance > 0 && userWallet.Balance >= topupTransaction.Amount)
                                {
                                    await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, topupTransaction.RechargeAmount, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.TransactionID).ConfigureAwait(false);

                                    //await _walletRepository.saveTransaction((int)TransactionType.Debit, topupTransaction.RechargeAmount, transactionRemark, userWallet.Id);

                                }

                                response.Message = "Transaction updated successfully";
                            }
                            else
                            {
                                transactionRemark = "Credited after topup failed transaction";
                                string title = "Top Up";
                                string body = "";
                                var vednFailedMessage = vendResponse.Message;
                                await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendFailed.ToString(), (int)PaymentStatus.Failed, vendResponse.Response, topupTransaction.Id, vendResponse.Token, vendResponse.keyChangeToken, vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif, vendResponse.VendReference).ConfigureAwait(false);
                                //await _walletRepository.AddBalanceAsync(topupTransaction.UserId, topupTransaction.RechargeAmount,userWallet.Balance, transactionRemark).ConfigureAwait(false);
                                if (!topupTransaction.UseWallet)
                                {
                                    await _walletRepository.AddBalanceAsync(topupTransaction.UserId, topupTransaction.RechargeAmount, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount, topupTransaction.TransactionID).ConfigureAwait(false);
                                    AddOrUpdateNotificationsQuery newNotification = new()
                                    {
                                        UserID = topupTransaction.UserId,
                                        Title = "Top Up Failed",
                                        Description = "Top up failed Amount : R " + topupTransaction.RechargeAmount + " credited to wallet balance.",
                                        IsRead = (int)StatusEnum.Sent,
                                        NotificationType = (int)NotificationType.Updated

                                    };
                                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                                    // body = transactionRemark + " Credited Amt : R" + topupTransaction.RechargeAmount;


                                    mailBody = company.Name +
                                                      "<br>Transaction failed reason: " + vednFailedMessage +
                                                      "<br>Transaction remark: " + transactionRemark +
                                                      "<br>Amt: R " + topupTransaction.Amount +
                                                      "<br>Transaction fee: R " + topupTransaction.TransactionFee +
                                                      "<br>Recharge amount: R " + topupTransaction.RechargeAmount;
                                    body = mailBody;

                                    await SendEmailMessage(topupTransaction, user, body).ConfigureAwait(false);
                                }
                                if (topupTransaction.UseWallet && userWallet.Balance < topupTransaction.Amount)
                                {
                                    double amountToAdd = topupTransaction.RechargeAmount - userWallet.Balance;
                                    await _walletRepository.AddBalanceAsync(topupTransaction.UserId, amountToAdd, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount, topupTransaction.TransactionID).ConfigureAwait(false);
                                    AddOrUpdateNotificationsQuery newNotification = new()
                                    {
                                        UserID = topupTransaction.UserId,
                                        Title = "Top Up Failed",
                                        Description = "Top up failed Amount : R " + amountToAdd + " credited to wallet balance.",
                                        IsRead = (int)StatusEnum.Sent,
                                        NotificationType = (int)NotificationType.Updated
                                    };
                                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

                                    mailBody = company.Name +
                                                      "\nTransaction failed reason: " + vednFailedMessage +
                                                      "\nTransaction remark: " + transactionRemark +
                                                      "\nAmt: R " + topupTransaction.Amount +
                                                      "\nTransaction fee: R " + topupTransaction.TransactionFee +
                                                      "\nRecharge amount: R " + topupTransaction.RechargeAmount;
                                    body = mailBody;
                                    await SendEmailMessage(topupTransaction, user, body).ConfigureAwait(false);
                                }
                                if (topupTransaction.UseWallet && userWallet.Balance > topupTransaction.RechargeAmount)
                                {
                                    await _walletRepository.AddBalanceAsync(topupTransaction.UserId, 0, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount, topupTransaction.TransactionID).ConfigureAwait(false);
                                }

                                if (!string.IsNullOrEmpty(deviceToken) && deviceToken != "string")
                                {
                                    await _pushNotification.SendMessage(title, body, deviceToken, topupTransaction.UserId).ConfigureAwait(false);
                                }

                            }
                        }
                    }
                    return response;
                    //}
                }
                else
                {
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(PayFastModel.m_payment_id),
                        ErrorMessage = "Transaction not exist or transaction is already processed."
                    });
                }
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);
            }
            catch (Exception ex)
            {
                throw ex;

                //var applicationLogger = new ApplicationLogger
                //{
                //    Request = JsonConvert.SerializeObject(request),
                //    Method = "payfastmodel",
                //    Error = ex.Message
                //};
                //_ = await _documentRepository.AddApplicationLogger(applicationLogger).ConfigureAwait(false);
            }
            return response;

        }

        //Notify wallet transaction only
        public async Task<VendRequestResponse> Handle(WalletTopUpModel request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new UpdateWalletTopUpTransactionQueryValidator(_topUpRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new VendRequestResponse();
            int result = 0;

            var topupTransaction = await _topUpRepository.GetTopupTransactionDetails(request.m_payment_id);
            var user = await _userRepository.GetUserById(topupTransaction.UserId).ConfigureAwait(true);
            var companyDetails = new CompanyDetailsDto();
            try
            {
                companyDetails = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, user.CompanyId.ToString());
                throw;
            }

            if (string.IsNullOrEmpty(topupTransaction.PayFastResponse) && !string.IsNullOrEmpty(topupTransaction.TopupStatus) && topupTransaction.TopupStatus == "TrailVendSuccess" && string.IsNullOrEmpty(topupTransaction.VendResponse) && string.IsNullOrEmpty(topupTransaction.ReceiptNumber))
            {
                LogToFile(companyDetails.WWWPath, "validated txn not processed previously");
                //if (topupTransaction.id > 0)
                //{

                result = await _topUpRepository.UpdateWalletTopupTransactions(request).ConfigureAwait(false);

                await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.PayFastSuccess.ToString(), (int)PaymentStatus.Failed, null, topupTransaction.Id, null, null, null, null, null, null, null, null).ConfigureAwait(false);

                if (result > 0)
                {

                    response.StatusCode = 200;
                    string transactionRemark = "";
                    //var topupTransaction = await _topUpRepository.GetTopupTransactionDetails(request.m_payment_id);

                    if (topupTransaction != null)
                    {

                        var company = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
                        var userWallet = await _walletRepository.GetUserWalletById(topupTransaction.UserId);
                        var walletAmountToUse = userWallet.Balance;

                        string vendPayMethod = "";

                        vendPayMethod = "none";

                        LogToFile(companyDetails.WWWPath, "before process recharge ");
                        var vendResponse = await _vendRequestHelper.ProcessRecharge(topupTransaction, vendPayMethod).ConfigureAwait(true);
                        LogToFile(companyDetails.WWWPath, "after process recharge");

                        response = vendResponse;
                        string deviceToken = await _userRepository.GetDeviceToken(topupTransaction.UserId).ConfigureAwait(false);
                        string mailBody = "";
                        if (vendResponse.StatusCode == 200)
                        {

                            await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendSuccess.ToString(), (int)PaymentStatus.Complete, vendResponse.Response, topupTransaction.Id, vendResponse.Token, vendResponse.keyChangeToken,
                                                                            vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif, vendResponse.VendReference).ConfigureAwait(false);


                            var sendMessage = new DownloadPurchaceRecieptPdfQuery
                            {
                                TransactionId = topupTransaction.TransactionID
                            };
                            await _topUpRepository.UpdateTransactionUseWallet(topupTransaction.Id).ConfigureAwait(false);
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
                                                "<br>Payment received." +
                                                "<br>Meter: " + PurchaseDetails.MeterNumber +
                                                "<br>RCT: " + PurchaseDetails.ReceiptId +
                                                "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                "<br>Token: " + PurchaseDetails.StandardTokens +
                                                "<br>Units: " + PurchaseDetails.Units;
                                        body = mailBody;
                                    }
                                    if (!string.IsNullOrEmpty(PurchaseDetails.BsstToken))
                                    {
                                        var amount = PurchaseDetails.BsstTokenAmount.ToString().Replace(",", ".");
                                        mailBody = company.Name +
                                                 "<br>Payment received." +
                                                 "<br>RCT: " + PurchaseDetails.ReceiptId +
                                                 "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2);
                                        body = mailBody;
                                    }
                                    else
                                    {
                                        var amount = PurchaseDetails.Amount.ToString().Replace(",", ".");
                                        mailBody = company.Name +
                                                  "<br>Payment received." +
                                                  "<br>RCT: " + PurchaseDetails.ReceiptId +
                                                  "<br>Amt: R " + Math.Round(Convert.ToDecimal(amount) / 100, 2) +
                                                  "<br>Remaining bal: R " + Math.Round(Convert.ToDecimal(userWallet.Balance) / 100, 2);
                                        body = mailBody;


                                    }

                                    await _pushNotification.SendMessage(title, body, deviceToken, topupTransaction.UserId).ConfigureAwait(false);



                                }
                                var mailReq = new DownloadPurchaceRecieptPdfQuery()
                                {
                                    TransactionId = topupTransaction.TransactionID,

                                };
                                await _topUpRepository.DownloadPurchaceRecieptPdfQuery(mailReq, topupTransaction.UserId).ConfigureAwait(false);


                                AddOrUpdateNotificationsQuery newNotification = new()
                                {
                                    UserID = topupTransaction.UserId,
                                    Title = "Top Up successfull",
                                    Description = "Top up successfull, recharged with amount: R " + topupTransaction.Amount + " .",
                                    IsRead = (int)StatusEnum.Sent,
                                    NotificationType = (int)NotificationType.Updated

                                };
                                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                            }

                            transactionRemark = "Debited after Topup Successfull Transaction";
                            if (userWallet.Balance > 0 && userWallet.Balance <= topupTransaction.Amount)
                            {
                                await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, userWallet.Balance, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.TransactionID).ConfigureAwait(false);

                                //await _walletRepository.saveTransaction((int)TransactionType.Debit, userWallet.Balance, transactionRemark, userWallet.Id);
                            }
                            if (userWallet.Balance > 0 && userWallet.Balance >= topupTransaction.Amount)
                            {
                                await _walletRepository.DeductBalanceAsync(topupTransaction.UserId, topupTransaction.RechargeAmount, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.TransactionID).ConfigureAwait(false);

                                //await _walletRepository.saveTransaction((int)TransactionType.Debit, topupTransaction.RechargeAmount, transactionRemark, userWallet.Id);

                            }

                            response.Message = "Transaction updated successfully";
                        }
                        else
                        {
                            transactionRemark = "Credited after topup failed transaction";
                            string title = "Top Up";
                            string body = "";
                            var vednFailedMessage = vendResponse.Message;
                            await _topUpRepository.UpdateTransactionStatus(TopUpStatusEnum.VendFailed.ToString(), (int)PaymentStatus.Failed, vendResponse.Response, topupTransaction.Id, vendResponse.Token, vendResponse.keyChangeToken, vendResponse.bsstToken, vendResponse.mrktMsg, vendResponse.customerMsg, vendResponse.ReceiptNumber, vendResponse.Tarrif, vendResponse.VendReference).ConfigureAwait(false);
                            //await _walletRepository.AddBalanceAsync(topupTransaction.UserId, topupTransaction.RechargeAmount,userWallet.Balance, transactionRemark).ConfigureAwait(false);
                            if (!topupTransaction.UseWallet)
                            {
                                await _walletRepository.AddBalanceAsync(topupTransaction.UserId, topupTransaction.RechargeAmount, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount, topupTransaction.TransactionID).ConfigureAwait(false);
                                AddOrUpdateNotificationsQuery newNotification = new()
                                {
                                    UserID = topupTransaction.UserId,
                                    Title = "Top Up Failed",
                                    Description = "Top up failed Amount : R " + topupTransaction.RechargeAmount + " credited to wallet balance.",
                                    IsRead = (int)StatusEnum.Sent,
                                    NotificationType = (int)NotificationType.Updated

                                };
                                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                                // body = transactionRemark + " Credited Amt : R" + topupTransaction.RechargeAmount;


                                mailBody = company.Name +
                                                  "<br>Transaction failed reason: " + vednFailedMessage +
                                                  "<br>Transaction remark: " + transactionRemark +
                                                  "<br>Amt: R " + topupTransaction.Amount +
                                                  "<br>Transaction fee: R " + topupTransaction.TransactionFee +
                                                  "<br>Recharge amount: R " + topupTransaction.RechargeAmount;
                                body = mailBody;

                                await SendEmailMessage(topupTransaction, user, body).ConfigureAwait(false);
                            }
                            if (topupTransaction.UseWallet && userWallet.Balance < topupTransaction.Amount)
                            {
                                double amountToAdd = topupTransaction.RechargeAmount - userWallet.Balance;
                                await _walletRepository.AddBalanceAsync(topupTransaction.UserId, amountToAdd, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount, topupTransaction.TransactionID).ConfigureAwait(false);
                                AddOrUpdateNotificationsQuery newNotification = new()
                                {
                                    UserID = topupTransaction.UserId,
                                    Title = "Top Up Failed",
                                    Description = "Top up failed Amount : R " + amountToAdd + " credited to wallet balance.",
                                    IsRead = (int)StatusEnum.Sent,
                                    NotificationType = (int)NotificationType.Updated
                                };
                                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

                                mailBody = company.Name +
                                                  "<br>Transaction failed reason: " + vednFailedMessage +
                                                  "<br>Transaction remark: " + transactionRemark +
                                                  "<br>Amt: R " + topupTransaction.Amount +
                                                  "<br>Transaction fee: R " + topupTransaction.TransactionFee +
                                                  "<br>Recharge amount: R " + topupTransaction.RechargeAmount;
                                body = mailBody;
                                await SendEmailMessage(topupTransaction, user, body).ConfigureAwait(false);
                            }
                            if (topupTransaction.UseWallet && userWallet.Balance > topupTransaction.RechargeAmount)
                            {
                                await _walletRepository.AddBalanceAsync(topupTransaction.UserId, 0, userWallet.Balance, transactionRemark, topupTransaction.UseWallet, topupTransaction.Amount, topupTransaction.TransactionID).ConfigureAwait(false);
                            }

                            if (!string.IsNullOrEmpty(deviceToken) && deviceToken != "string")
                            {
                                await _pushNotification.SendMessage(title, body, deviceToken, topupTransaction.UserId).ConfigureAwait(false);
                            }



                        }
                    }
                }
                return response;
                //}
            }
            else
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(PayFastModel.m_payment_id),
                    ErrorMessage = "Transaction not exist or transaction is already processed."
                });
            }
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            return response;

        }

        public async Task<string> Handle(UpdatePaymentMethodsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new UpdatePaymentMethodsQueryValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            await _topUpRepository.UpdatePaymentMethods(request, _workContext.CurrentUserId).ConfigureAwait(false);

            return "Updated successfully!";
        }

        public async Task<CancelTransactionResponseModel> Handle(CancelTopUpTransactionQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var res = new CancelTransactionResponseModel();
            var commonValidator = new CancelTopUpTransactionQueryValidator(_topUpRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);


            await _topUpRepository.CancelTopUpTransaction(request).ConfigureAwait(false);
            res.ResMessage = "Transaction cancelled by user!";

            return res;
        }
        public async Task SendEmailMessage(GetTopUpTransaction topupTransaction, UserProfileDto user, string body)
        {
            var communications = await _communicationRepository.GetCommunicationsByUserId(topupTransaction.UserId).ConfigureAwait(false);
            foreach (var communication in communications)
            {
                var name = communication.Name?.ToLower(); // To handle case-insensitive check

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

        static void LogToFile(string filePath, string message)
        {

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string fileName = "topUpError.txt";
            filePath = Path.Combine(filePath, fileName);
            if (!File.Exists(filePath))
            {
                // Create the file
                using (FileStream fs = File.Create(filePath))
                {
                    // Optionally write something to the file
                    byte[] content = new UTF8Encoding(true).GetBytes("File created successfully.");
                    fs.Write(content, 0, content.Length);
                }
                Console.WriteLine("File created.");
            }
            // Append log with timestamp
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(filePath, logEntry); // Append log to file
        }
    }
}
