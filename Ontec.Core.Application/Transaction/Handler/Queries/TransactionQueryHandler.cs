using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc.Html;
using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto.AccountTransactions;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.Domain.Requests.Transaction.Queries;
using SelectPdf;


namespace Ontec.Core.Application.Transaction.Handler.Queries
{
    public class TransactionQueryHandler : IRequestHandler<GetTransactionMasterQuery, TransactionMasterDto>
                                            , IRequestHandler<GetTransactionSummaryQuery, TransactionSummaryDto>
                                            , IRequestHandler<DownloadStatementPdfQuery, TransactionStatementResponseModel>

    {
        private readonly IUserRepository _userRepository;
        private IHostingEnvironment Environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IWorkContext _workContext;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly IDocumentRepository _documentRepository;
        private readonly IOtpService _otpService;
        public readonly ITopUpRepository _topUpRepository;
        public readonly IGenericRepository _genericRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;

        public TransactionQueryHandler(IUserRepository userRepository
                                       , ICompanyRepository companyRepository
                                       , IPropertyRepository propertyRepository
                                       , IWorkContext workContext
                                       , IMeterRepository meterRepository
                                      , MasterApiSetting masterApiSetting
                                      , IMasterApiConnectService masterApiConnectService
                                      , IDocumentRepository documentRepository
                                    , IHostingEnvironment _environment
                                   , IHttpContextAccessor httpContextAccessor
                                    , IOtpService otpService
                                    , ITopUpRepository topUpRepository
            , IGenericRepository genericRepository,
IEmailTemplateRepository emailTemplateRepository)
        {
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
            _companyRepository = companyRepository;
            _workContext = workContext;
            _meterRepository = meterRepository;
            _masterApiConnectService = masterApiConnectService;
            _masterApiSetting = masterApiSetting;
            _documentRepository = documentRepository;
            Environment = _environment;
            _httpContextAccessor = httpContextAccessor;
            _otpService = otpService;
            _topUpRepository = topUpRepository;
            _genericRepository = genericRepository;
            _emailTemplateRepository = emailTemplateRepository;
        }
        public async Task<TransactionMasterDto> Handle(GetTransactionMasterQuery request, CancellationToken cancellationToken)
        {

            request.TrimAllStrings();

            var commonValidator = new GetTransactionMasterQueryValidator(_userRepository, _companyRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var enumList = EnumHelper.GetSelectList(typeof(TransactionPeriod));

            TransactionMasterDto masterDto = new();
            var periodList = new List<OntecSelectListItem>();
            foreach (var period in enumList.ToList())
            {
                periodList.Add(new OntecSelectListItem
                {
                    Id = int.Parse(period.Value),
                    Name = period.Text,
                });
            }
            masterDto.TransactionPeriod = periodList;
            bool isAdmin = false;

            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                isAdmin = true;
            }
            masterDto.PropertyList = await _propertyRepository.GetTransactionPropertyList(request.UserId, isAdmin);

            return masterDto;
        }

        public async Task<TransactionSummaryDto> Handle(GetTransactionSummaryQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetTransactionSummaryQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            TransactionSummaryDto summaryDto = new()
            {
                ClosingBalance = 0,
                OpeningBalance = 0,
                TotalAdjustments = 0,
                TotalBillingcalculations = 0,
                TotalDeposit = 0,
                Transaction = new List<TransactionData>()
            };
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = DateTime.UtcNow.Date;
            switch (request.TransactionCycleId)
            {
                case 1://30 days
                    startDate = startDate.AddDays(-30);
                    break;

                case 2://60 days
                    startDate = startDate.AddDays(-60);
                    break;

                case 3://90 days
                    startDate = startDate.AddDays(-90);
                    break;

                case 4://custom
                    //startDate = request.FromDate.Date.AddDays(1);
                    startDate = request.FromDate.Date;
                    //endDate = request.Todate.Date.AddDays(1);
                    endDate = request.Todate.Date;
                    break;

                default:
                    startDate = request.FromDate.Date;
                    endDate = request.Todate.Date.AddDays(1);
                    break;
            }
            var meterList = await _meterRepository.GetMetersByPropertyId(request.PropertyId).ConfigureAwait(false);
            var customerAgreementId = string.Empty;
            var customerAccountId = string.Empty;
            foreach (var meter in meterList)
            {
                if (string.IsNullOrEmpty(customerAgreementId))
                {
                    var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                    var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                    if (meterResult != null && meterResult.Data.Count() > 0)
                    {
                        var transactionStatementDto = new TransactionStatementDto();
                        var hasValidData = meterResult?.Data?.Any(x => x.Customer != null && x.CustomerAccount != null && x.CustomerAgreement != null) == true;
                        if (hasValidData)
                        {
                            var item = meterResult?.Data.First(x => x.Customer != null && x.CustomerAccount != null && x.CustomerAgreement != null);
                            var meterId = meterResult?.Data[0].Meter.Id;
                            customerAccountId = meterResult?.Data[0].CustomerAccount.Id;
                            customerAgreementId = meterResult?.Data[0].CustomerAgreement.Id;
                            var dateFilter = "&filter=(dateEntered)(GTE)(" + startDate.Date.ToString("yyyy-MM-dd") + "T00:00:000.000%2B0200)" +
                                              "&filter=(dateEntered)(LT)(" + endDate.Date.ToString("yyyy-MM-dd") + "T23:59:599.000%2B0200)";
                            var ctdateFilter = "&filter=(transDate)(GTE)(" + startDate.Date.ToString("yyyy-MM-dd") + "T00:00:000.000%2B0200)" +
                                              "&filter=(transDate)(LT)(" + endDate.Date.ToString("yyyy-MM-dd") + "T23:59:599.000%2B0200)";

                            var paging = "&paging=(limit)(250)(offset)(0)";
                            var accountTransactionUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AccountTransApi + "?filter=(customerAccountId)(EQ)(" + customerAccountId + ")" + paging + dateFilter;

                            var accountTrasactions = await _masterApiConnectService.GetAccountTransactions(accountTransactionUrl).ConfigureAwait(false);


                            if (accountTrasactions != null)
                            {


                                var sortedData = accountTrasactions.Data.OrderBy(t => t.TransDate).ToList();

                                if (sortedData.Any())
                                {
                                    summaryDto.OpeningBalance = Math.Round(sortedData.First().ResultantBalance, 2);

                                    summaryDto.ClosingBalance = Math.Round(sortedData.Last().ResultantBalance, 2);
                                }
                                var accountTransTypeList = (from t in accountTrasactions.Data
                                                            group t by t.AccountTransType into g
                                                            select new
                                                            {
                                                                AccountTransType = g.Key.ToString(),
                                                                ResultantBalance = g.Sum(t => t.ResultantBalance)
                                                            }).AsEnumerable();
                                //if (accountTransTypeList.Any())
                                //{
                                //    foreach (var type in accountTransTypeList)
                                //    {
                                //        if (type.AccountTransType == "BILLING_CALC")
                                //        {

                                //            summaryDto.TotalBillingcalculations = Math.Round(type.ResultantBalance,2);
                                //        }
                                //        if (type.AccountTransType == "ADJUSTMENT")
                                //        {
                                //            summaryDto.TotalAdjustments = Math.Round(type.ResultantBalance, 2);
                                //        }
                                //        if (type.AccountTransType == "DEPOSIT")
                                //        {
                                //            summaryDto.TotalDeposit = Math.Round(type.ResultantBalance, 2);
                                //        }
                                //    }
                                //}
                                var accountAdjustments = (from t in accountTrasactions.Data
                                                          where t.AccountTransType == "ADJUSTMENT"
                                                          select new AccountAdjustment
                                                          {
                                                              Date = t.DateEntered.ToString("dd-MM-yyyy"),
                                                              Total = t.ResultantBalance
                                                          }).AsEnumerable();
                                var adjustmentTotal = new Total
                                {
                                    TotalR = accountAdjustments.Sum(t => t.Total)
                                };


                                transactionStatementDto.AdjustmentTotal = adjustmentTotal;
                                transactionStatementDto.AccountAdjustments = accountAdjustments;
                            }

                            var transactionStatement = await GetNewTransaction(customerAgreementId, ctdateFilter, meter.MeterNumber, dateFilter, customerAccountId);
                            var totalAmount = transactionStatement.LastOrDefault()?.TotalAmount;
                            summaryDto.OpeningBalance = Convert.ToDecimal(summaryDto.OpeningBalance) - Convert.ToDecimal(totalAmount);
                            if (transactionStatement.Any())
                            {
                                foreach (var transaction in transactionStatement)
                                {
                                    if (transaction.AccountTransType == "BILLING_CALC")
                                    {

                                        summaryDto.TotalBillingcalculations += Math.Round(transaction.TotalAmount, 2);
                                    }
                                    if (transaction.AccountTransType == "ADJUSTMENT")
                                    {
                                        summaryDto.TotalAdjustments += Math.Round(transaction.TotalAmount, 2);
                                    }
                                    if (transaction.AccountTransType.Contains("DEPOSIT"))
                                    {
                                        summaryDto.TotalDeposit += Math.Round(transaction.TotalAmount, 2);
                                    }
                                }
                            }


                            summaryDto.Transaction = transactionStatement;

                        }
                    }
                }

                
            }
            return summaryDto;
        }


        public async Task<TransactionStatementResponseModel> Handle(DownloadStatementPdfQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new DownloadStatementPdfQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var res = new TransactionStatementResponseModel();

            try
            {
                TransactionSummaryDto summaryDto = new()
                {
                    ClosingBalance = 0,
                    OpeningBalance = 0,
                    TotalAdjustments = 0,
                    TotalBillingcalculations = 0,
                    TotalDeposit = 0,
                    Transaction = new List<TransactionData>()
                };
                TransactionStatementDto transactionStatementDto = new();
                DateTime startDate = DateTime.UtcNow.Date;
                DateTime endDate = DateTime.UtcNow.Date;
                switch (request.TransactionCycleId)
                {
                    case 1://30 days
                        startDate = startDate.AddDays(-30);
                        break;
                    case 2://60 days
                        startDate = startDate.AddDays(-60);
                        break;
                    case 3://90 days
                        startDate = startDate.AddDays(-90);
                        break;

                    case 4://custom
                        //startDate = request.FromDate.Date.AddDays(1);
                        //endDate = request.Todate.Date.AddDays(1);

                        startDate = request.FromDate.Date;
                        endDate = request.Todate.Date;
                        break;

                    default:
                        startDate = request.FromDate.Date;
                        endDate = request.Todate.Date.AddDays(1);
                        break;
                }
                var customerAgreementId = "";
                var customerAccountId = "";
                var customerAccountName = "";
                var meterList = await _meterRepository.GetMetersByPropertyId(request.PropertyId).ConfigureAwait(false);

                foreach (var meter in meterList)
                {
                    if (string.IsNullOrEmpty(customerAgreementId))
                    {
                        var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                        var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                        if (meterResult != null)
                        {
                            var meterId = meterResult.Data[0].Meter.Id;
                            customerAccountId = meterResult.Data[0].CustomerAccount.Id;
                            customerAccountName = meterResult.Data[0].CustomerAccount.AccountName;

                            customerAgreementId = meterResult.Data[0].CustomerAgreement.Id;

                            var dateFilter = "&filter=(dateEntered)(GTE)(" + startDate.Date.ToString("yyyy-MM-dd") + "T00:00:000.000%2B0200)" +
                                         "&filter=(dateEntered)(LT)(" + endDate.Date.ToString("yyyy-MM-dd") + "T23:59:599.000%2B0200)";
                            var ctdateFilter = "&filter=(transDate)(GTE)(" + startDate.Date.ToString("yyyy-MM-dd") + "T00:00:000.000%2B0200)" +
                                              "&filter=(transDate)(LT)(" + endDate.Date.ToString("yyyy-MM-dd") + "T23:59:599.000%2B0200)";

                            var paging = "&paging=(limit)(250)(offset)(0)";

                            var accountTransactionUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AccountTransApi + "?filter=(customerAccountId)(EQ)(" + customerAccountId + ")" + paging + dateFilter;

                            var accountTrasactions = await _masterApiConnectService.GetAccountTransactions(accountTransactionUrl).ConfigureAwait(false);



                            if (accountTrasactions != null)
                            {
                                //var sortedData = accountTrasactions.Data.OrderByDescending(t => t.Id);
                                //if (sortedData.Any())
                                //{
                                //    //summaryDto.OpeningBalance = Convert.ToDecimal(string.Format("{0:F2}", sortedData.LastOrDefault().ResultantBalance));
                                //    summaryDto.OpeningBalance = Math.Round(sortedData.LastOrDefault()?.ResultantBalance ?? 0m, 2, MidpointRounding.AwayFromZero);
                                //    summaryDto.ClosingBalance = Math.Round(sortedData.FirstOrDefault()?.ResultantBalance ?? 0m, 2, MidpointRounding.AwayFromZero);

                                //    //summaryDto.ClosingBalance = Convert.ToDecimal(string.Format("{0:F2}", sortedData.FirstOrDefault().ResultantBalance));
                                //}
                                var sortedData = accountTrasactions.Data.OrderBy(t => t.TransDate).ToList();

                                if (sortedData.Any())
                                {
                                    summaryDto.OpeningBalance = Math.Round(sortedData.First().ResultantBalance, 2);

                                    summaryDto.ClosingBalance = Math.Round(sortedData.Last().ResultantBalance, 2);
                                }
                                var accountTransTypeList = (from t in accountTrasactions.Data
                                                            group t by t.AccountTransType into g
                                                            select new
                                                            {
                                                                AccountTransType = g.Key.ToString(),
                                                                ResultantBalance = g.Sum(t => t.ResultantBalance)
                                                            }).AsEnumerable();

                                var accountAdjustments = (from t in accountTrasactions.Data
                                                          where t.AccountTransType == "ADJUSTMENT"
                                                          select new AccountAdjustment
                                                          {
                                                              Date = t.DateEntered.ToString("dd-MM-yyyy"),
                                                              Total = t.ResultantBalance
                                                          }).AsEnumerable();
                                var adjustmentTotal = new Total
                                {
                                    TotalR = accountAdjustments.Sum(t => t.Total)
                                };

                            }

                            var transactionStatement = await GetNewTransaction(customerAgreementId, ctdateFilter, meter.MeterNumber, dateFilter,customerAccountId).ConfigureAwait(false);

                            var totalAmount = transactionStatement.LastOrDefault()?.TotalAmount;
                            summaryDto.OpeningBalance = Convert.ToDecimal(summaryDto.OpeningBalance) - Convert.ToDecimal(totalAmount);
                            if (transactionStatement.Any())
                            {
                                foreach (var transaction in transactionStatement)
                                {
                                    if (transaction.AccountTransType == "BILLING_CALC")
                                    {

                                        summaryDto.TotalBillingcalculations += Math.Round(transaction.TotalAmount, 2);
                                    }
                                    if (transaction.AccountTransType == "ADJUSTMENT")
                                    {
                                        summaryDto.TotalAdjustments += Math.Round(transaction.TotalAmount, 2);
                                    }
                                    if (transaction.AccountTransType.Contains("DEPOSIT"))
                                    {
                                        summaryDto.TotalDeposit += Math.Round(transaction.TotalAmount, 2);
                                    }
                                }
                            }
                            summaryDto.Transaction = transactionStatement;

                        }
                    }
                }
                var propertyOwner = await _propertyRepository.GetPropertyUserByMeterId(request.PropertyId).ConfigureAwait(false);

                if (propertyOwner != null)
                {
                    transactionStatementDto.PropertyUserDetails = propertyOwner;
                    if (customerAccountName != null)
                    {
                        transactionStatementDto.PropertyUserDetails.Account = customerAccountName;
                    }

                }

                byte[] pdfBytes = await NewGeneratePdfWithSelectPdf(transactionStatementDto, summaryDto, request.PropertyId, startDate, endDate).ConfigureAwait(false);

                MemoryStream stream = new MemoryStream();
                stream.Write(pdfBytes, 0, pdfBytes.Length);
                stream.Position = 0;
                string wwwPath = this.Environment.WebRootPath;
                string contentPath = this.Environment.ContentRootPath;
                var requestPath = _httpContextAccessor.HttpContext.Request;
                var domain = $"{requestPath.Scheme}://{requestPath.Host}";

                var absoluteUrl = domain + "/uploads/";
                string path = this.Environment.WebRootPath + "/uploads";
                absoluteUrl += "documents/TransactionStatements/";
                path = this.Environment.WebRootPath + "/uploads/documents/TransactionStatements";

                string fileName = request.PropertyId.ToString();
                string extension = ".pdf";
                fileName += extension;

                path = Path.Combine(path);
                absoluteUrl += fileName;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (File.Exists(path + "/" + fileName))
                {
                    await DeleteFileAsync(path, fileName);
                }

                using (StreamWriter sw = new StreamWriter(path + "/" + fileName, true))
                {
                    stream.CopyTo(sw.BaseStream);
                }
                if (File.Exists(path + "/" + fileName))
                {
                    if (request.Type == "download")
                    {
                        res.DocumentUrl = absoluteUrl;
                        res.ResponseMsg = "Download document";
                        byte[] fileBytes = null;


                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(res.DocumentUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var Doc = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(res.DocumentUrl),
                                Type = Path.GetExtension(res.DocumentUrl),
                                Document = fileBytes
                            };

                            res.TransactionStatement = Doc;
                            res.DocumentUrl = null;
                        }
                    }
                    if (request.Type == "email")
                    {
                        var user = await _userRepository.GetUserById(_workContext.CurrentUserId).ConfigureAwait(false);
                        string relativePath = path + "/" + fileName;
                        EmailModelClass obj = new()
                        {
                            title = "Transaction Statement",
                            email = user.Email,
                            forEvent = "DownloadStatement",
                            subtitle = "",
                            companyId = user.CompanyId,
                            mobile = user.Mobile,
                            propertyUser = user.FirstName,
                            body = "",
                            documentPath = relativePath
                        };

                        res.ResponseMsg = await _otpService.SendTransactionStatement(obj).ConfigureAwait(false);

                    }
                }
            }
            catch (Exception ex)
            {
                res.ResponseMsg = ex.Message;
                var applicationLogger = new ApplicationLogger
                {
                    Request = JsonConvert.SerializeObject(request),
                    Method = "DownloadStatementPdfQuery_Handle",
                    Error = ex.Message
                };
                _ = await _documentRepository.AddApplicationLogger(applicationLogger).ConfigureAwait(false);
            }
            return res;

        }

        private async Task<TransactionStatement> GetTransaction(string meterId, string dateFilter, string meterNumber)
        {
            var customerTransUrl = _masterApiSetting.BaseUrl + _masterApiSetting.CustomerTransApi + "?meterId=" + meterId
                + "&paging=(limit)(250)(offset)(0)" + dateFilter + "&filter=(customerTransType)(EQ)(SCHEDULED)";

            var data = await _masterApiConnectService.GetCustomerTransactions(customerTransUrl).ConfigureAwait(false);

            var meter = await _meterRepository.GetMeterByMeterNumber(meterNumber).ConfigureAwait(false);
            var transactionStatementDto = new TransactionStatement();
            if (meter != null)
            {
                transactionStatementDto = new TransactionStatement()
                {
                    MeterNumber = meterNumber,
                    MeterType = meter.MeterType,
                    Unit = meter.Unitofmeasure
                };

                if (data != null)
                {
                    var transactionModel = (from t in data.Data
                                            select new TransactionModel
                                            {
                                                Usage = t.CustomerTransItems.Sum(x => x.Units),
                                                Cost = t.CustomerTransItems.Sum(x => x.AmtInclTax),
                                                TransactionDate = t.TransDate.ToString("dd-MM-yyyy"),
                                                Network = 0,
                                                Vending = 0,
                                                Vat = 0,
                                                TotalR = t.CustomerTransItems.Sum(x => x.AmtInclTax)
                                            }).AsEnumerable();
                    transactionStatementDto.Transactions = transactionModel;
                    transactionStatementDto.Totals = new Total
                    {
                        Usage = transactionModel.Sum(t => t.Usage),
                        Cost = transactionModel.Sum(t => t.Cost),
                        Network = transactionModel.Sum(t => t.Network),
                        Vending = transactionModel.Sum(t => t.Vending),
                        Vat = transactionModel.Sum(t => t.Vat),
                        TotalR = transactionModel.Sum(t => t.TotalR)
                    };
                }
            }
            return transactionStatementDto;
        }
        private async Task<IEnumerable<TransactionData>> GetNewTransaction(string customerAgreementId, string dateFilter, string meterNumber, string acctranDateFilter, string customerAccountId)
        {
            var transactionStatementDto = new TransactionStatement();
            var AccountReference = "";
            var ResultantBalance = "";
            var OurReference = "";
            double txnFee = 0;
            string txnId = string.Empty;
            var paging = "&paging=(limit)(250)(offset)(0)";

            var scheduledCustomerTransUrl = _masterApiSetting.BaseUrl + _masterApiSetting.CustomerTransApi + "?customerAgreementId=" + customerAgreementId
                + "&paging=(limit)(250)(offset)(0)" + dateFilter + "&filter=(customerTransType)(EQ)(SCHEDULED)";
            var vendCustomerTransUrl = _masterApiSetting.BaseUrl + _masterApiSetting.CustomerTransApi + "?customerAgreementId=" + customerAgreementId
              + "&paging=(limit)(250)(offset)(0)" + dateFilter + "&filter=(customerTransType)(EQ)(VEND)";

            var data = new TransactionMasterApiModel()
            {
                Data = new List<Domain.Models.Dto.Transaction.Transaction>()
            };
            //var scheduleddata = await _masterApiConnectService.GetCustomerTransactions(scheduledCustomerTransUrl).ConfigureAwait(false);
            //var getData = await _masterApiConnectService.GetCustomerTransactions(vendCustomerTransUrl).ConfigureAwait(false);
            //if (getData != null)
            //    data.Data.AddRange(getData.Data);
            //if (scheduleddata != null)
            //    data.Data.AddRange(scheduleddata.Data);


            var scheduledTask = _masterApiConnectService.GetCustomerTransactions(scheduledCustomerTransUrl);

            var vendTask = _masterApiConnectService.GetCustomerTransactions(vendCustomerTransUrl);

            await Task.WhenAll(scheduledTask, vendTask);

            var scheduleddata = await scheduledTask;

            var getData = await vendTask;
            if (getData != null)
                data.Data.AddRange(getData.Data);
            if (scheduleddata != null)
                data.Data.AddRange(scheduleddata.Data);

            var transactionTypeMap = new Dictionary<string, string>
                    {
                        { "stdToken", "Standard Token" },
                        { "fixed", "Fixed Costs" },
                        { "bsstToken", "FBE Token" },
                        { "aux", "Auxiliary payment" },
                        { "dep", "Deposit" },
                        { "tou", "Consumption Charge" },
                        { "demand", "Demand Charge" },
                        { "refund", "Refund" },
                        { "bsstRepeat", "FBE Repeat" }
                    };

            var accountTrasactions = new AccountTransactionApiModel();
            var accountAdjustmentTransactions = new AccountTransactionApiModel();
            var result = new List<TransactionData>();

            //var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
            //var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);

            //if (meterResult != null)
            //{
            //    var meterId = meterResult.Data[0].Meter.Id;
            //    var customerAccountId = meterResult.Data[0].CustomerAccount.Id;
            if (!string.IsNullOrEmpty(customerAccountId))
            {
                var accountTransactionUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AccountTransApi + "?filter=(customerAccountId)(EQ)(" + customerAccountId + ")" + paging + acctranDateFilter;
                accountTrasactions = await _masterApiConnectService.GetAccountTransactions(accountTransactionUrl).ConfigureAwait(false);
            }
            var accountMap = accountTrasactions.Data.Where(x => !string.IsNullOrEmpty(x.AccountRef))
                           .GroupBy(x => x.AccountRef)
                           .ToDictionary(g => g.Key, g => g.First());
            //}

            //if (data != null && accountTrasactions != null)
            //{
            //    // data.Data = data.Data.OrderByDescending(t => t.TransDate).ToList();
            //    //data.Data = data.Data.OrderBy(t => t.ReceiptNum).ToList();
            //    //accountTrasactions.Data = accountTrasactions.Data.OrderBy(t => t.AccountRef).ToList();
            //    //foreach (var tran in data.Data)
            //    //{
            //    //    decimal resultantBalance = 0;
            //    //    string TransactionType = "";

            //    //    var balance = accountTrasactions.Data
            //    //                            .FirstOrDefault(t => t.AccountRef != null && t.AccountRef.Equals(tran.ReceiptNum, StringComparison.OrdinalIgnoreCase));
            //    //    var TransType = accountTrasactions.Data
            //    //                            .FirstOrDefault(t => t.AccountRef != null && t.AccountRef.Equals(tran.ReceiptNum, StringComparison.OrdinalIgnoreCase));



            //    //    //var balance = accountTrasactions.Data.Where(t => t.AccountRef != null).FirstOrDefault(t => t.AccountRef.Equals(tran.ReceiptNum));
            //    //    //var accountTransType= accountTrasactions.Data.Where(t => t.AccountTransType != null).FirstOrDefault(t => t.AccountTransType.Equals(tran.ReceiptNum));
            //    //    if (balance != null)
            //    //    {
            //    //        resultantBalance = Convert.ToDecimal(string.Format("{0:F2}", balance.ResultantBalance));
            //    //    }
            //    //    if (TransType != null)
            //    //    {
            //    //        TransactionType = TransType.AccountTransType;
            //    //    }

            //    var transactionModel = (from t in data.Data
            //                            select new TransactionModel
            //                            {
            //                                Usage = t.CustomerTransItems.Sum(x => x.Units),
            //                                Cost = t.CustomerTransItems.Sum(x => x.AmtInclTax),
            //                                TransactionDate = t.TransDate.ToString("dd-MM-yyyy"),
            //                                Network = 0,
            //                                Vending = 0,
            //                                Vat = 0,
            //                                TotalR = t.CustomerTransItems.Sum(x => x.AmtInclTax)
            //                            }).AsEnumerable();
            //    transactionStatementDto.Transactions = transactionModel;
            //    transactionStatementDto.Totals = new Total
            //    {
            //        Usage = transactionModel.Sum(t => t.Usage),
            //        Cost = transactionModel.Sum(t => t.Cost),
            //        Network = transactionModel.Sum(t => t.Network),
            //        Vending = transactionModel.Sum(t => t.Vending),
            //        Vat = transactionModel.Sum(t => t.Vat),
            //        TotalR = transactionModel.Sum(t => t.TotalR)
            //    };
            //    data.Data = data.Data.OrderByDescending(t => t.ReceiptNum).ToList();
            //    accountTrasactions.Data = accountTrasactions.Data.OrderByDescending(t => t.AccountRef).ToList();

            //    accountAdjustmentTransactions = new AccountTransactionApiModel
            //    {
            //        Data = accountTrasactions.Data
            //           .Where(t => t.AccountRef != null && t.AccountTransType.Equals("ADJUSTMENT", StringComparison.OrdinalIgnoreCase))
            //           .ToList()
            //    };

            //    accountAdjustmentTransactions.Data = accountAdjustmentTransactions.Data.OrderByDescending(t => t.AccountRef).ToList();

            //    foreach (var tran in data.Data)
            //    {

            //        decimal resultantBalance = 0;
            //        string TransactionType = "";

            //        var balance = accountTrasactions.Data
            //                       .FirstOrDefault(t => t.AccountRef != null && t.AccountRef.Equals(tran.ReceiptNum, StringComparison.OrdinalIgnoreCase));
            //        var transType = accountTrasactions.Data
            //                            .FirstOrDefault(t => t.AccountRef != null && t.AccountRef.Equals(tran.ReceiptNum, StringComparison.OrdinalIgnoreCase));


            //        double txnFee = await _topUpRepository.GetTransactionNoFeeFromRctNum(tran.ReceiptNum);
            //        string TxnId = await _topUpRepository.GetTransactionNoFromRctNum(tran.ReceiptNum);
            //        if (balance != null)
            //        {
            //            resultantBalance = Convert.ToDecimal(string.Format("{0:F2}", balance.ResultantBalance));
            //        }
            //        if (transType != null)
            //        {
            //            TransactionType = transType.AccountTransType;
            //        }
            //        var transaction = new TransactionData
            //        {
            //            Date = tran.TransDate.ToString("dd-MM-yyyy HH:mm"),
            //            Meter = tran.ServiceResource + " (" + tran.MeterNumber + ")",
            //            TaxAmount = Convert.ToDecimal(string.Format("{0:F}", tran.AmtTax)),
            //            TotalAmount = Convert.ToDecimal(string.Format("{0:F2}", tran.AmtInclTax)),
            //            ResultantBalance = resultantBalance,
            //            AccountTransType = TransactionType == "DEPOSIT" ? TransactionType + "(" + tran.ReceiptNum + ")" : TransactionType,
            //            ReceiptNumber = tran.ReceiptNum,
            //            //OurRef=tran.o
            //            TransactionId = TxnId,
            //            TransactionFee = txnFee,
            //            Tariff = tran.Tariff,




            //        };

            //        if (transaction.AccountTransType.Contains("DEPOSIT"))
            //        {

            //            transaction.TotalAmount = +transaction.TotalAmount;

            //        }
            //        else
            //        {
            //            transaction.TotalAmount = -transaction.TotalAmount;
            //        }
            //        if (tran.CustomerTransItems != null && tran.CustomerTransItems.Any())
            //        {
            //            decimal tenderedAmount = 0;
            //            transaction.Unit = tran.CustomerTransItems.Sum(t => t.Units);
            //            var details = new List<TCustomerTransItem>();
            //            foreach (var item in tran.CustomerTransItems)
            //            {
            //                transactionTypeMap.TryGetValue(item.TransItemType, out var typeDescription);

            //                details.Add(new TCustomerTransItem
            //                {

            //                    Amount = Convert.ToDecimal(string.Format("{0:F2}", item.AmtInclTax)),
            //                    VAT = Convert.ToDecimal(string.Format("{0:F2}", item.AmtTax)),
            //                    Description = item.Description,
            //                    Type = typeDescription ?? "Unknown Type",
            //                    Units = item.Units,
            //                    Tariff = item.Tariff

            //                });
            //                if (transaction.AccountTransType.Contains("DEPOSIT"))
            //                {
            //                    tenderedAmount += Convert.ToDecimal(string.Format("{0:F2}", item.AmtInclTax));
            //                }
            //            }
            //            if (transaction.AccountTransType.Contains("DEPOSIT"))
            //            {
            //                tenderedAmount += Convert.ToDecimal(txnFee);
            //            }
            //            transaction.Details = details;
            //            transaction.TenderedAmount = tenderedAmount;
            //            var depositTransaction = transaction.Details
            //                                    .FirstOrDefault(t => t.Type != null && t.Type.Equals("Deposit", StringComparison.OrdinalIgnoreCase));

            //            if (depositTransaction != null)
            //            {
            //                transaction.TotalAmount = depositTransaction.Amount;
            //            }

            //        }

            //        result.Add(transaction);
            //    }

            //    foreach (var accTran in accountAdjustmentTransactions.Data)
            //    {

            //        decimal resultantBalance = 0;
            //        string TransactionType = "";
            //        string comment = "";
            //        var balance = accountTrasactions.Data
            //                                         .FirstOrDefault(t => t.AccountRef != null && t.AccountTransType.Equals("Adjustment", StringComparison.OrdinalIgnoreCase));
            //        var transType = accountTrasactions.Data
            //                            .FirstOrDefault(t => t.AccountRef != null && t.AccountTransType.Equals("Adjustment", StringComparison.OrdinalIgnoreCase));
            //        if (balance != null)
            //        {
            //            resultantBalance = Convert.ToDecimal(string.Format("{0:F2}", balance.ResultantBalance));
            //        }
            //        if (transType != null)
            //        {
            //            TransactionType = transType.AccountTransType;
            //            comment = transType.Comment;
            //        }
            //        var transaction = new TransactionData
            //        {
            //            Date = accTran.TransDate.ToString("dd-MM-yyyy HH:mm"),
            //            Meter = "Account Adjustment",
            //            TaxAmount = Convert.ToDecimal(string.Format("{0:F2}", accTran.AmtTax)),
            //            TotalAmount = Convert.ToDecimal(string.Format("{0:F2}", accTran.AmtInclTax)),
            //            ResultantBalance = resultantBalance,
            //            AccountTransType = TransactionType,
            //            OurRef = accTran.OurRef,
            //            Comment = comment,
            //            Tariff = accTran.Tariff,

            //        };
            //        if (transaction.AccountTransType.Contains("DEPOSIT"))
            //        {
            //            transaction.TotalAmount = +transaction.TotalAmount;

            //        }

            //        //if (transaction.AccountTransType.Contains("BILLING_CALC"))
            //        //{
            //        //    if (transaction.TotalAmount.ToString().Contains("-"))
            //        //    {
            //        //        transaction.TotalAmount = transaction.TotalAmount;
            //        //    }
            //        //    else
            //        //    {
            //        //        transaction.TotalAmount = -transaction.TotalAmount;
            //        //    }
            //        //}


            //        result.Add(transaction);
            //    }
            //}




            if (data != null && accountTrasactions != null)
            {
                accountAdjustmentTransactions = new AccountTransactionApiModel
                {
                    Data = accountTrasactions.Data.Where(t => t.AccountRef != null && t.AccountTransType.Equals("ADJUSTMENT", StringComparison.OrdinalIgnoreCase)).ToList()
                };

                decimal reverseEnergyTotalAmount = 0;
                decimal reverseEnergyTotalTax = 0;
                decimal reverseEnergyTotalUnits = 0;

                var receiptNumbers = data.Data.Where(x => !string.IsNullOrEmpty(x.ReceiptNum)).Select(x => x.ReceiptNum).Distinct().ToList();

                var transactionFees = await _topUpRepository.GetTransactionFeesByReceiptNumbers(receiptNumbers).ConfigureAwait(false);

                foreach (var tran in data.Data)
                {
                    decimal resultantBalance = 0;
                    string TransactionType = "";

                    accountMap.TryGetValue(tran.ReceiptNum, out var accountTran);

                    transactionFees.TryGetValue(tran.ReceiptNum, out var resultDto);

                    if (resultDto != null)
                    {
                        txnFee = resultDto.transaction_fee;
                        txnId = resultDto.transaction_id;
                    }
                    else
                    {
                        txnFee = 0;
                        txnId = string.Empty;
                    }

                    if (accountTran != null)
                    {
                        resultantBalance = Convert.ToDecimal(string.Format("{0:F2}", accountTran.ResultantBalance));
                        TransactionType = accountTran.AccountTransType;
                    }

                    var transaction = new TransactionData
                    {
                        Date = tran.TransDate.ToString("dd-MM-yyyy HH:mm"),
                        Meter = tran.ServiceResource + " (" + tran.MeterNumber + ")",
                        TaxAmount = Convert.ToDecimal(string.Format("{0:F2}", tran.AmtTax)),
                        TotalAmount = Convert.ToDecimal(string.Format("{0:F2}", tran.AmtInclTax)),
                        ResultantBalance = resultantBalance,
                        AccountTransType = TransactionType == "DEPOSIT" ? TransactionType + "(" + tran.ReceiptNum + ")" : TransactionType,
                        ReceiptNumber = tran.ReceiptNum,
                        TransactionId = txnId,
                        TransactionFee = txnFee
                    };

                    if (!string.IsNullOrEmpty(transaction.AccountTransType) && transaction.AccountTransType.Contains("DEPOSIT"))
                    {
                        transaction.TotalAmount = +transaction.TotalAmount;
                    }
                    else
                    {
                        transaction.TotalAmount = -transaction.TotalAmount;
                    }

                    if (tran.CustomerTransItems != null && tran.CustomerTransItems.Any())
                    {
                        decimal tenderedAmount = 0;
                        transaction.Unit = tran.CustomerTransItems.Sum(t => t.Units);
                        var details = new List<TCustomerTransItem>();

                        foreach (var item in tran.CustomerTransItems)
                        {
                            transactionTypeMap.TryGetValue(item.TransItemType, out var typeDescription);
                            

                            if (!string.IsNullOrEmpty(transaction.AccountTransType) && transaction.AccountTransType.Contains("DEPOSIT"))
                            {
                                tenderedAmount += Convert.ToDecimal(string.Format("{0:F2}", item.AmtInclTax));
                            }
                            //if (transaction.AccountTransType.Contains("DEPOSIT"))
                            //{
                            //    tenderedAmount += Convert.ToDecimal(txnFee);
                            //}
                            transactionTypeMap.TryGetValue(item.TransItemType, out typeDescription);
                            string resolvedType = typeDescription ?? "Unknown Type";

                            details.Add(
                                         new TCustomerTransItem
                                         {
                                             Amount = Convert.ToDecimal(string.Format("{0:F2}", item.AmtInclTax)),
                                             VAT = Convert.ToDecimal(string.Format("{0:F2}", item.AmtTax)),
                                             Description = item.Description,
                                             Type = resolvedType,
                                             Units = item.Units,
                                             TransactionItemType = item.TransItemType
                                         });


                        }


                        if (!string.IsNullOrEmpty(transaction.AccountTransType) && transaction.AccountTransType.Contains("DEPOSIT"))
                        {
                            tenderedAmount += Convert.ToDecimal(txnFee);
                        }

                        transaction.Details = details;
                        transaction.TenderedAmount = tenderedAmount;
                        var depositTransaction = transaction.Details.FirstOrDefault(t => t.Type != null && t.Type.Equals("Deposit", StringComparison.OrdinalIgnoreCase));

                        if (depositTransaction != null)
                        {
                            transaction.TotalAmount = depositTransaction.Amount;
                        }

                    }

                    result.Add(transaction);
                }

                foreach (var accTran in accountAdjustmentTransactions.Data)
                {
                    var transaction = new TransactionData
                    {
                        Date = accTran.TransDate.ToString("dd-MM-yyyy HH:mm"),
                        Meter = "Account Adjustment",
                        TaxAmount = Convert.ToDecimal( string.Format("{0:F2}", accTran.AmtTax)),
                        TotalAmount = Convert.ToDecimal( string.Format("{0:F2}", accTran.AmtInclTax)),
                        ResultantBalance = Convert.ToDecimal( string.Format("{0:F2}", accTran.ResultantBalance)),
                        AccountTransType = accTran.AccountTransType,
                        Comment = accTran.Comment,
                        OurRef = accTran.OurRef,
                        Tariff = accTran.Tariff
                    };
                    result.Add(transaction);
                }
            }
            result = result.OrderByDescending(t => DateTime.ParseExact(t.Date, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture)) .ToList();
            return result;
        }


        private async Task<byte[]> NewGeneratePdfWithSelectPdf(TransactionStatementDto transactionStatementDto, TransactionSummaryDto summaryDto, int meterId, DateTime From, DateTime To)
        {
            decimal elecVat = 0;
            decimal elecIncludeVat = 0;


            decimal waterVat = 0;
            decimal waterIncludeVat = 0;


            decimal gasVat = 0;
            decimal gasIncludeVat = 0;
            var company = await _companyRepository.GetCompanyDetails(_workContext.CurrentCompanyId).ConfigureAwait(false);
            string relativePath = "";
            if (transactionStatementDto.PropertyUserDetails != null)
            {
                var requestPath = _httpContextAccessor.HttpContext.Request;
                var domain = $"{requestPath.Scheme}://{requestPath.Host}";
                string wwwPath = this.Environment.WebRootPath;
                string contentPath = this.Environment.ContentRootPath;
                transactionStatementDto.PropertyUserDetails.CompanyLogo = domain + transactionStatementDto.PropertyUserDetails.CompanyLogo;
                relativePath = transactionStatementDto.PropertyUserDetails.CompanyLogo.Replace(domain, wwwPath).Replace("/", "\\");//uri.LocalPath;
                //byte[] imageByte = System.IO.File.ReadAllBytes(relativePath);
                //string imgbase64 = Convert.ToBase64String(imageByte);

                //string imageUrl = "data:image/png;base64, " + imgbase64;
            }
            string logoUrl = transactionStatementDto.PropertyUserDetails.CompanyLogo;
            #region css
            string cssStyles = @"<style>
                                body {
                                  font-family: Arial, sans-serif;
                                  background: #fff;
                                  margin: 0;
                                  padding: 20px;
                                }

                                .container {
                                  max-width: 900px;
                                  margin: auto;
                                  padding: 20px;
                                  border: 1px solid #ccc;
                                  background: white;

                                }


                                .header {
                                  display: flex;
                                  justify-content: space-between;
                                  align-items: start;
                                  width: 100%;  


                                }

                                .logo {
                                  width:150px; 
                                display: flex;
                                justify-content: center;
                                justify-items: center;
                                }

                                .company-details {
                                  background: #d1e4ec;
                                  padding: 20px;
                                  border-radius: 5px;
                                  width: 70%;

                                }

                                .company-details-text{
                                display: flex;
                                gap:10px;

                                }

                                .section-title {
                                  text-align: center;
                                  margin-top: 35px; 
                                  border-bottom: 0.5px solid #222 !important;
                                  padding-bottom: 20px;
                                color: #003E52;
                                font-size: 20px;
                                font-weight: bold;
                                }

                                .info-table td {
                                  padding: 5px 10px;

                                }
                                .summary th {
                                color: #00334e;
                                font-weight: bold;
                                font-size: 17px;
                                padding-top: 8px;
                                padding-bottom: 8px;
                                padding-left: 10px;
                                }


                                .table-row-bg {
                                background: F1F2F4;
                                border : 1px solid #B7B7B7;
                                }

                                .summary-table, .payment-table, .reading-table {
                                  width: 100%;
                                  border-collapse: collapse;
                                  margin: 20px 0;
                                }

                                .summary-table th,
                                .payment-table th
                                {
                                  background: #009C50;
                                  color: white;
                                  text-align: center;
                                  font-size: 15px;
                                  padding: 8px;

                                }

                                .gray-row td {
                                  background-color: #F1F2F4;
                                }
                                table, td, th {
                                  border-collapse: collapse;
                                  border-spacing: 0;
                                }

                                .reading-table th {
                                background: #F7B67E;
                                 color: black;
                                  text-align: center;
                                  font-size: 15px;
                                  padding: 8px;
                                }
                                .reading-table td {
 
                                  text-align: center;
                                  font-size: 12px;
                                  padding: 8px;
                                }

                                .summary tr.bg-gray td {
                                  background-color: #F1F2F4;
                                }

                                .reading-table tr.total td {
                                  border-top: 1px solid #F7B67E;
                                  border-bottom: 1px solid #F7B67E;
                                  border-left: none;
                                  border-right: none;
                                    }
                                .payment-table tr.total td {
                                border-top: 1px solid #009C50;
                                border-bottom: 1px solid #009C50;
                                }


                                .summary-table td,
                                .payment-table td {
                                  /* border-bottom: 1px solid #009C50; */
                                  text-align: center;
                                  font-size: 12px;
                                  padding: 8px;
                                }

                                .sub-header {
                                  background: #eee;
                                  font-weight: bold;
                                }

                                .total {
                                  font-weight: bold;

                                }

                                .footer {
                                  text-align: center;
                                  background: #00334e;
                                  color: white;
 
                                padding: 10px 0; 
                                  margin-top: 40px;
                                  margin-left: -20px;
                                  margin-right: -20px;
                                }
                            .reading-table {
                              width: 100%;
                              table-layout: fixed;   /* 🔥 REQUIRED */
                            }
                            .tariff-cell {
                              white-space: normal !important;
                              word-break: break-word !important;
                              overflow-wrap: anywhere !important;
                            }
                                .reading-table tr:nth-child(even):not(:first-child) td {
                                  background-color: #ffffff;
                                }
                                .reading-table tr:nth-child(odd):not(:first-child) td {
                                  background-color: #F1F2F4;
                                }
                                .payment-table tr:nth-child(even):not(:first-child) td {
                                  background-color: #ffffff;
                                }
                                .payment-table tr:nth-child(odd):not(:first-child) td {
                                  background-color: #F1F2F4;
                                }
                    </style> ";
            #endregion
            var filtered = summaryDto.Transaction.ToList()
                                .Where(t => {
                                    var type = t.AccountTransType?.Split('(')[0].Trim();
                                    return !string.Equals(type, "DEPOSIT", StringComparison.OrdinalIgnoreCase);
                                })
                                .ToList();
            var groupedTransaction = filtered.GroupBy(t => t.Meter);
            if (groupedTransaction != null)
            {
                elecVat = groupedTransaction
                               .Where(g => g.Key.Contains("ELEC", StringComparison.OrdinalIgnoreCase))
                               .Sum(g => g.Sum(t => t.TaxAmount));
                elecIncludeVat = groupedTransaction
                          .Where(g => g.Key.Contains("ELEC", StringComparison.OrdinalIgnoreCase))
                          .Sum(g => g.Sum(t => t.TotalAmount)) + elecVat;

                waterVat = groupedTransaction
                           .Where(g => g.Key.Contains("Water", StringComparison.OrdinalIgnoreCase))
                           .Sum(g => g.Sum(t => t.TaxAmount));
                waterIncludeVat = groupedTransaction
                          .Where(g => g.Key.Contains("Water", StringComparison.OrdinalIgnoreCase))
                          .Sum(g => g.Sum(t => t.TotalAmount)) + waterVat;

                gasVat = groupedTransaction
                          .Where(g => g.Key.Contains("Gas", StringComparison.OrdinalIgnoreCase))
                          .Sum(g => g.Sum(t => t.TaxAmount));
                gasIncludeVat = groupedTransaction
                          .Where(g => g.Key.Contains("Gas", StringComparison.OrdinalIgnoreCase))
                          .Sum(g => g.Sum(t => t.TotalAmount)) + gasVat;

            }

            decimal openingBalance = Math.Round(summaryDto.OpeningBalance, 2);
            decimal closingBalance = Math.Round(summaryDto.ClosingBalance, 2);
            decimal totalDeposit = Math.Round(summaryDto.TotalDeposit, 2);
            decimal totalAdjustment = Math.Round(summaryDto.TotalAdjustments, 2);
            decimal totalBillingCalculations = Math.Round(summaryDto.TotalBillingcalculations, 2);
            //decimal totalAuxillary = Math.Round(summaryDto.TotalAuxiliary, 2);
            CultureInfo zaCulture = new CultureInfo("en-Us");

            string formattedOpenBalance = openingBalance.ToString("N2", zaCulture);
            string formattedCloseBalance = closingBalance.ToString("N2", zaCulture);
            string formattedTotalDeposit = totalDeposit.ToString("N2", zaCulture);
            string formattedAdjustment = totalAdjustment.ToString("N2", zaCulture);
            string htmlContent = cssStyles + @"
           
<div class='container'>
    <header class='header'>
      <div  class='logo-section' style='margin-left: 20px; margin-right: 5px; width: 30%; '>
        <img src='" + logoUrl + @"' alt='Meerkat Utilities Logo' class='logo' style='margin-left: auto; margin-right: auto;'>

      </div>
      <div class='company-details' style='width: 70%;'>
        <h2 style='width: 100%;'>" + company.CompanyName + @"</h2>
        <div class='company-details-text' style='font-size: 12px; font-family: sans-serif;'> 
        <div><p style='width:370px;  line-height: 18px; '>" + company.Address + @"</p></div>
        <div><p style='width:100% ;  line-height: 18px; '>Tel: " + company.MobileNumber + @"<br/> VAT No: " + company.VAT + @"</p></div></div>       
      </div>
    </header>


<h2 class='section-title' >TRANSACTION STATEMENT</h2>
 <p style='text-align: center;'>" + From.ToString("dd MMMM yyyy") + " To " + To.ToString("dd MMMM yyyy") + @"</p>
<hr>
<div class='divider'></div>
<div style='width: 100%; display: flex; padding: 0 5px 30px 5px; border-bottom: 0.5px solid black; gap: 40px; '>


<div style='width: 50%; margin-top: 10px;'>
<table style='width: 100%; border-collapse: collapse;table-layout: fixed; '>
      <tr>
        <td style='font-weight: 550; padding-bottom: 20px; width: 150px; font-size: 15px;'><b>Customer Name</b></td>
        <td style='padding-bottom: 20px; width: 10px; font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px; font-size: 15px;'>" + transactionStatementDto.PropertyUserDetails.Owner + @"</td>
      </tr>
      <tr>
        <td style='font-weight: 550; padding-bottom: 20px; width: 150px; font-size: 15px;'><b>Complex</b></td>
        <td style='padding-bottom: 20px; width: 10px; font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px; font-size: 15px;'>" + transactionStatementDto.PropertyUserDetails.Complex + @"</td>
      </tr>
      <tr>
        <td style='font-weight: 550; padding-bottom: 20px; width: 150px;font-size: 15px;'><b>Address</b></td>
        <td style='padding-bottom: 20px;  width: 10px; font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px; font-size: 15px;'>" + transactionStatementDto.PropertyUserDetails.Address + @"</td>
      </tr>
      <tr>
        <td style='font-weight: 550; padding-bottom: 20px; width: 150px; font-size: 15px;'><b>Generated Date</b></td>
        <td style='padding-bottom: 20px;  width: 10px;font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px;font-size: 15px;'>" + DateTime.UtcNow.ToString("dd/MM/yyyy") + @"</td>
      </tr>

    </table>
</div>


<div style='width: 50%; margin-top: 10px;'>
    <table style='width: 100%; border-collapse: collapse; table-layout: fixed;'>
      <tr>
        <td style='font-weight:550; padding-bottom: 20px;  width: 100px; font-size: 15px;'><b>Account No</b></td>
        <td style='padding-bottom: 20px;  width: 10px; font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px; font-size: 15px;'> R" + transactionStatementDto.PropertyUserDetails.Account + @"</td>
      </tr>
      <tr>
        <td style='font-weight: 550; padding-bottom: 20px; width: 100px; font-size: 15px;'><b>Email</b></td>
        <td style='padding-bottom: 20px; width: 10px; font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px; font-size: 15px;'>" + transactionStatementDto.PropertyUserDetails.Email + @"</td>
      </tr>
      <tr>
        <td style='font-weight: 550; padding-bottom: 20px; width: 100px; font-size: 15px;'><b>Contact</b></td>
        <td style='padding-bottom: 20px;  width: 10px; font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px; font-size: 15px;'>" + transactionStatementDto.PropertyUserDetails.ContactNo + @"</td>
      </tr>
    <tr>
        <td style='font-weight: 550; padding-bottom: 20px; width: 150px; font-size: 15px;'><b>VAT Number</b></td>
        <td style='padding-bottom: 20px;  width: 10px;font-size: 15px;'>:</td>
        <td style='font-weight: normal; padding-bottom: 20px;font-size: 15px;'>" + transactionStatementDto.PropertyUserDetails.TaxNumber + @"</td>
      </tr>
    </table>
  </div>
</div>  
<div class='divider'></div>
<hr>
</style>
<table class='summary' style='width:100%; margin-top:20px;'>
<tr style='color:003E52 ; text-align: left; width: 100%;'>
<th style='width: 55%; color: 003E52; '>Summary</th>
<th style='width: 23%;color: 003E52; text-align: left; padding-left: 14px;'>VAT</th>
<th style='width: 22%; color: 003E52; text-align: center;'>Amount Incl VAT</th>
</tr>

<tr style='font-weight: bold; font-size: 12px; padding-top: 5px; padding-bottom: 5px; border: 1px solid #B7B7B7; ' class='bg-gray'>
<td style='padding-top: 5px; padding-bottom: 5px; padding-left: 10px;'>Opening Balance </td>
<td style=''></td>
<td style='text-align: center; padding-right: 8px;'> R " + formattedOpenBalance + @"</td>
</tr>
<tr style=' font-size: 12px; '>
<td style='padding-top: 5px; padding-bottom: 5px;font-weight: bold; padding-left: 10px;' >Payment Received</td>
<td></td>
<td style='text-align: center; padding-right: 8px; padding-top:5px; padding-bottom: 5px;'> R " + formattedTotalDeposit  + @"</td>
</tr>
<tr style=' font-size: 12px; '>
<td style='padding-top: 5px; padding-bottom: 5px;font-weight: bold; padding-left: 10px;' >Adjustments</td>
<td></td>
<td style='text-align: center; padding-right: 8px; padding-top:5px; padding-bottom: 5px;'> R " + formattedAdjustment + @"</td>
</tr>
<tr style='font-weight: bold; font-size: 13px; padding-top: 5px; padding-bottom: 5px; background-color: #F1F2F4; '>
<td style='padding-top: 5px; padding-bottom: 5px; padding-left: 10px;'>Utilities </td>
<td style='width: 23%;color: 003E52; text-align: left; padding-left: 14px;'>VAT</td>
<td style='width: 22%; color: 003E52; text-align: center;'>Amount Excl VAT</td>

</tr>

<tr style='font-size: 12px; padding-top: 10px; padding-bottom: 10px; '>
<td style='padding-left: 23px; padding-top: 5px; padding-bottom: 5px;'>Electricity</td>
<td style='padding-left: 5px; padding-top: 5px; padding-bottom: 5px;'>R " + elecVat.ToString("N2", new CultureInfo("en-Us")) + @"</td>
<td style='padding-left: 5px; padding-top: 5px; padding-bottom: 5px;text-align: center;'>R " + elecIncludeVat.ToString("N2",new CultureInfo("en-Us")) + @"</td>
</tr>
<tr style='font-size: 12px; padding-top: 10px; padding-bottom: 10px; '>
<td style='padding-left: 23px; padding-top: 5px; padding-bottom: 5px; '>Water</td>
<td style='padding-left: 5px; padding-top: 5px; padding-bottom: 5px; '>R " + waterVat.ToString("N2", new CultureInfo("en-Us")) + @"</td>
<td style='padding-left: 5px; padding-top: 5px; padding-bottom: 5px;text-align: center; '>R " + waterIncludeVat.ToString("N2", new CultureInfo("en-Us")) + @"</td>
</tr>
<tr style='font-size: 12px; padding-top: 5px; padding-bottom: 5px; '>
<td style='padding-left: 23px;'>Gas</td>
<td style='padding-left: 5px;'>R " + gasVat.ToString("N2", new CultureInfo("en-Us")) + @"</td>
<td style='padding-left: 5px;text-align: center;'>R " + gasIncludeVat.ToString("N2", new CultureInfo("en-Us")) + @"</td>
</tr>

<tr style='font-weight: bold; font-size: 13px; padding-top: 5px; padding-bottom: 5px; background-color: #F1F2F4; border: 1px solid #B7B7B7;'>
<td  style='padding-top: 5px; padding-bottom: 5px; padding-left: 10px;'>Closing Balance </td>
<td style='font-size: 12px ;padding-top: 5px; padding-bottom: 5px;'></td>
<td style='font-size: 12px; padding-top: 5px; padding-bottom: 5px;text-align: center;'>R " + formattedCloseBalance + @"</td>
</tr>
</table>";


            var depositTransactions = summaryDto.Transaction
                                    .Where(t => t.AccountTransType.Contains("DEPOSIT", StringComparison.OrdinalIgnoreCase)).ToList();


            if (depositTransactions.Count() > 0)
            {
                double totDepTax = 0;
                double totDepAmount = 0;

                htmlContent += @"<h3 style = 'color: #003E52;margin-top: 50px; font-size: 18px;'> Payments </h3>
                                <table class='payment-table'>
                                  <tr><th>Date</th>
                                        <th>Receipt Number</th>
                                        <th>VAT</th><th>Amount Incl VAT</th>
                                        </tr>";

                foreach (var deptxn in depositTransactions)
                {


                    totDepTax += Convert.ToDouble(deptxn.TaxAmount);
                    totDepAmount += Convert.ToDouble(deptxn.TotalAmount);

                    htmlContent += @"<tr><td>" + deptxn.Date.Split(' ')[0] + @"</td>
                                    <td>" + deptxn.ReceiptNumber + @"</td>
                                    <td>R" + deptxn.TaxAmount.ToString("N2", new CultureInfo("en-Us")) + @"</td>
                                    <td>R " + deptxn.TotalAmount.ToString("N2", new CultureInfo("en-Us")) + @"</td></tr>";


                }
                htmlContent += @"<tr class='total'><td colspan='2' style='text-align: left; padding-left: 80px; font-size: 14px;'>Total</td>";
                htmlContent += @"<td>R " + Math.Round(totDepTax, 4).ToString("N2", new CultureInfo("en-Us")) + @"</td>";
                htmlContent += @"<td>R " + Math.Round(totDepAmount, 4).ToString("N2", new CultureInfo("en-Us")) + @" </td></tr>";
                htmlContent += @"</table>";

            }
            var adjustmentTransactions = summaryDto.Transaction.Where(t => t.AccountTransType.Contains("ADJUSTMENT", StringComparison.OrdinalIgnoreCase)).ToList();
            if (adjustmentTransactions.Count() > 0)
            {
                double totTax = 0;
                double totAmount = 0;
                htmlContent += @"<h3 style = 'color: #003E52;margin-top: 50px; font-size: 18px;'> Account Adjustments</h3>
                                <table class='payment-table'>
                                 <tr><th>Date</th>
                                    <th>Reference</th>
                                    <th>VAT</th>
                                    <th>Amount Incl VAT</th></tr>";
                foreach (var adjtxn in adjustmentTransactions)
                {

                    totTax += Convert.ToDouble(adjtxn.TaxAmount);
                    totAmount += Convert.ToDouble(adjtxn.TotalAmount);

                    htmlContent += @"<tr><td>" + adjtxn.Date.Split(' ')[0] + @"</td>
                                    <td>" + adjtxn.OurRef + @"</td>
                                    <td>R " + adjtxn.TaxAmount.ToString("N2", new CultureInfo("en-Us")) + @"</td>
                                    <td>R " + adjtxn.TotalAmount.ToString("N2", new CultureInfo("en-Us")) + @"</td></tr>";
                }
                htmlContent += @"<tr class='total'><td colspan='2' style='text-align: left; padding-left: 80px; font-size: 14px;'>Total</td>";
                htmlContent += @"<td>R " + Math.Round(totTax, 4).ToString("N2", new CultureInfo("en-Us")) + @"</b></td>";
                htmlContent += @"<td>R " + Math.Round(totAmount, 4).ToString("N2", new CultureInfo("en-Us")) + @" </td></tr>";
                htmlContent += @"</table>";


            }

            htmlContent += @"<div class='label' style='page-break-inside: avoid;page-break-inside: avoid;margin-bottom: 10px; margin-top: 10px; max-width: 1500px; text-align: left;'>";

            List<string> meterSections = new List<string>();


            var groupedTransactions = filtered.GroupBy(t => t.Meter);

            int rowCount = 0;
            double totalUnit = 0;
            double totalAmount = 0;
            double totaltariff = 0;
            foreach (var group in groupedTransactions)
            {
                var input = group.Key;
                //var start = input.IndexOf("(") + 1;
                //var end = input.IndexOf(")", start);
                //var value = input.Substring(start, end - start);


                var match = Regex.Match(input, @"\((.*?)\)");
                var value = match.Success ? match.Groups[1].Value : input;
                string unitOfMeasure = await _meterRepository.GetUnitOfMeasure(value).ConfigureAwait(false);

                if (!group.Key.Contains("ADJUSTMENT", StringComparison.OrdinalIgnoreCase))
                {
                    htmlContent += @"<h3 style='color: #003E52;margin-top: 50px; font-size: 15px;'>" + group.Key + @"</h3>"; // Meter number as heading
                    htmlContent += @"<table class='reading-table'>";
                    htmlContent += @"<tr><th>Date</th>";


                    htmlContent += @"<th class='tariff-cell'>Tariff</th>";
                    htmlContent += @"<th>Usage (" + (unitOfMeasure) + ") </th>";
                    htmlContent += @"<th>VAT</th>";
                    htmlContent += @"<th>Amount Incl VAT</th>";
                    htmlContent += @"</tr>";

                    var dayWiseGroups = group
                                        .GroupBy(t => t.Date.Split(' ')[0])
                                        .Select(g => new
                                        {
                                            Date = g.Key,
                                            Tariff = g
                                                    .SelectMany(x => x.Details)
                                                    .Select(x => x.Tariff)
                                                    .Distinct()
                                                    .FirstOrDefault(),
                                            TotalUnit = g.Sum(t => Convert.ToDouble(t.Unit)),
                                            TotalTaxAmount = g.Sum(t => Convert.ToDouble(t.TaxAmount)),
                                            TotalAmount = g.Sum(t => Convert.ToDouble(t.TotalAmount))
                                        })
                                        .OrderByDescending(g => DateTime.ParseExact(g.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture))
                                        .ToList();
                    foreach (var day in dayWiseGroups)
                    {
                        totalUnit += day.TotalTaxAmount;
                        totalAmount += day.TotalAmount;

                        htmlContent += @"<tr><td>" + day.Date + @"</td>";
                        htmlContent += @"<td class='tariff-cell'>" + day.Tariff + @"</td>";
                        htmlContent += @"<td>" + day.TotalUnit.ToString("N2", new CultureInfo("en-Us")) + @"</td>";
                        htmlContent += @"<td>R " + day.TotalTaxAmount.ToString("N2", new CultureInfo("en-Us")) + @"</td>";
                        htmlContent += @"<td>R " + day.TotalAmount.ToString("N2", new CultureInfo("en-Us")) + @"</td>";
                        htmlContent += @"</tr>";
                    }

                    //foreach (var transaction in group)
                    //{
                    //    totalUnit += Convert.ToDouble(transaction.TaxAmount);
                    //    totalAmount += Convert.ToDouble(transaction.TotalAmount);


                    //    htmlContent += @"<tr><td>" + transaction.Date.Split(' ')[0] + @"</td>";
                    //    htmlContent += @"<td>" + transaction.Tariff + @"</td>";
                    //    htmlContent += @"<td>" + transaction.Unit + @"</td>";

                    //    htmlContent += @"<td>R " + transaction.TaxAmount.ToString("0.00") + @"</td>";
                    //    htmlContent += @"<td>R " + transaction.TotalAmount.ToString("0.00") + @"</td>";
                    //    htmlContent += @"</tr>";
                    //}
                    rowCount++;

                    htmlContent += @"<tr class='total'><td colspan='3' style='text-align: left; padding-left: 80px; font-size: 14px;'>Total</td>";
                    htmlContent += @"<td>R " + Math.Round(totalUnit, 4).ToString("N2", new CultureInfo("en-Us")) + @"</td>";
                    htmlContent += @"<td>R " + Math.Round(totalAmount, 4).ToString("N2", new CultureInfo("en-Us")) + @" </td></tr>";
                    htmlContent += @"</table>";
                    totalUnit = 0;
                    totalAmount = 0;
                }


            }
            htmlContent += @"<br/><br/>";
            htmlContent += @"</div></div>";


            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1024;
            fullView.Options.MinPageLoadTime = 1;
            fullView.Options.MaxPageLoadTime = 3;
            fullView.Options.WebPageFixedSize = false;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            fullView.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
            fullView.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;

            SelectPdf.PdfDocument doc = fullView.ConvertHtmlString(htmlContent);

            //doc.Save()
            byte[] response;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //data.Save(ms);
                    doc.Save(ms);
                    response = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;

        }



        //private async Task<byte[]> dynamicGeneratePdfWithSelectPdf(TransactionStatementDto transactionStatementDto, TransactionSummaryDto summaryDto, int meterId, DateTime From, DateTime To)
        //{
        //    decimal elecVat = 0;
        //    decimal elecIncludeVat = 0;


        //    decimal waterVat = 0;
        //    decimal waterIncludeVat = 0;

        //    var txnStmtate = new EmailTemplateDto();
        //    decimal gasVat = 0;
        //    decimal gasIncludeVat = 0;
        //    var company = await _companyRepository.GetCompanyDetails(_workContext.CurrentCompanyId).ConfigureAwait(false);
        //    string relativePath = "";
        //    if (transactionStatementDto.PropertyUserDetails != null)
        //    {
        //        var requestPath = _httpContextAccessor.HttpContext.Request;
        //        var domain = $"{requestPath.Scheme}://{requestPath.Host}";
        //        string wwwPath = this.Environment.WebRootPath;
        //        string contentPath = this.Environment.ContentRootPath;
        //        transactionStatementDto.PropertyUserDetails.CompanyLogo = domain + transactionStatementDto.PropertyUserDetails.CompanyLogo;
        //        relativePath = transactionStatementDto.PropertyUserDetails.CompanyLogo.Replace(domain, wwwPath).Replace("/", "\\");//uri.LocalPath;
        //        //byte[] imageByte = System.IO.File.ReadAllBytes(relativePath);
        //        //string imgbase64 = Convert.ToBase64String(imageByte);

        //        //string imageUrl = "data:image/png;base64, " + imgbase64;
        //    }
        //    string logoUrl = transactionStatementDto.PropertyUserDetails.CompanyLogo;
        //    var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
        //    txnStmtate = emailTemplates.FirstOrDefault(g => g.Name.Equals("Transaction Statement"));

        //    var groupedTransaction = summaryDto.Transaction.GroupBy(t => t.Meter);
        //    if (groupedTransaction != null)
        //    {
        //        elecVat = groupedTransaction
        //                       .Where(g => g.Key.Contains("ELEC", StringComparison.OrdinalIgnoreCase))
        //                       .Sum(g => g.Sum(t => t.TaxAmount));
        //        elecIncludeVat = groupedTransaction
        //                  .Where(g => g.Key.Contains("ELEC", StringComparison.OrdinalIgnoreCase))
        //                  .Sum(g => g.Sum(t => t.TotalAmount)) + elecVat;

        //        waterVat = groupedTransaction
        //                   .Where(g => g.Key.Contains("Water", StringComparison.OrdinalIgnoreCase))
        //                   .Sum(g => g.Sum(t => t.TaxAmount));
        //        waterIncludeVat = groupedTransaction
        //                  .Where(g => g.Key.Contains("Water", StringComparison.OrdinalIgnoreCase))
        //                  .Sum(g => g.Sum(t => t.TotalAmount)) + waterVat;

        //        gasVat = groupedTransaction
        //                  .Where(g => g.Key.Contains("Gas", StringComparison.OrdinalIgnoreCase))
        //                  .Sum(g => g.Sum(t => t.TaxAmount));
        //        gasIncludeVat = groupedTransaction
        //                  .Where(g => g.Key.Contains("Gas", StringComparison.OrdinalIgnoreCase))
        //                  .Sum(g => g.Sum(t => t.TotalAmount)) + gasVat;

        //    }
        //    var payments = new List<Deposit>();
        //    var adjustments = new List<Adjustment>();
        //    var meterGroupTransactions = new List<GroupTransactions>();
        //    var model = new TransactionPdfViewModel
        //    {
        //        LogoUrl = logoUrl,
        //        CompanyName = company.CompanyName,
        //        CompanyAddress = company.Address,
        //        CompanyMobile = company.MobileNumber,
        //        VAT = company.VAT,
        //        TransactionPeriod = From.ToString("dd MMMM yyyy") + " To " + To.ToString("dd MMMM yyyy"),
        //        CustomerName = transactionStatementDto.PropertyUserDetails.Owner,
        //        CustomerComplex = transactionStatementDto.PropertyUserDetails.Complex,
        //        Address = transactionStatementDto.PropertyUserDetails.Address,
        //        GeneratedDate = DateTime.UtcNow.ToString("dd/MM/yyyy"),
        //        AccountNo = transactionStatementDto.PropertyUserDetails.Account,
        //        Email = transactionStatementDto.PropertyUserDetails.Email,
        //        Contact = transactionStatementDto.PropertyUserDetails.ContactNo,
        //        CustomerVatNo = transactionStatementDto.PropertyUserDetails.TaxNumber,
        //        OpeningBalance = Convert.ToDecimal(string.Format("{0:F2}", summaryDto.OpeningBalance)),
        //        TotalBillingcalculations = Convert.ToDecimal(string.Format("{0:F2}", summaryDto.TotalDeposit)),
        //        TotalDeposit = Convert.ToDecimal(string.Format("{0:F2}", summaryDto.TotalDeposit)),
        //        TotalAdjustments = Convert.ToDecimal(string.Format("{0:F2}", summaryDto.TotalAdjustments)),
        //        ElectricVat = elecVat,
        //        ElectricIncludeVat = elecIncludeVat,
        //        WaterVat = waterVat,
        //        WaterIncludeVat = waterIncludeVat,
        //        GasVat = gasVat,
        //        GasIncludeVat = gasIncludeVat,
        //        ClosingBalance = Convert.ToDecimal(string.Format("{0:F2}", summaryDto.ClosingBalance)),
        //    };



        //    var depositTransactions = summaryDto.Transaction
        //                            .Where(t => t.AccountTransType.Contains("DEPOSIT", StringComparison.OrdinalIgnoreCase)).ToList();

        //    var adjustmentTransactions = summaryDto.Transaction.Where(t => t.AccountTransType.Contains("ADJUSTMENT", StringComparison.OrdinalIgnoreCase)).ToList();



        //    if (depositTransactions.Count() > 0)
        //    {
        //        double totDepTax = 0;
        //        double totDepAmount = 0;

        //        foreach (var deptxn in depositTransactions)
        //        {


        //            totDepTax += Convert.ToDouble(deptxn.TaxAmount);
        //            totDepAmount += Convert.ToDouble(deptxn.TotalAmount);

        //            payments.Add(new Deposit
        //            {
        //                Date = deptxn.Date.Split(' ')[0],
        //                ReceiptNumber = deptxn.ReceiptNumber,
        //                TaxAmount = deptxn.TaxAmount,
        //                TotalAmount = deptxn.TotalAmount

        //            });

        //        }
        //        model.TotalDepAmount = Math.Round(totDepTax, 4).ToString("0.00");
        //        model.TotalDepTax = Math.Round(totDepAmount, 4).ToString("0.00");

        //    }

        //    if (adjustmentTransactions.Count() > 0)
        //    {
        //        double totTax = 0;
        //        double totAmount = 0;

        //        foreach (var adjtxn in adjustmentTransactions)
        //        {

        //            totTax += Convert.ToDouble(adjtxn.TaxAmount);
        //            totAmount += Convert.ToDouble(adjtxn.TotalAmount);
        //            adjustments.Add(new Adjustment
        //            {
        //                Date = adjtxn.Date.Split(' ')[0],
        //                ReceiptNumber = adjtxn.ReceiptNumber,
        //                TaxAmount = adjtxn.TaxAmount,
        //                TotalAmount = adjtxn.TotalAmount

        //            });
        //        }
        //        model.TotalAdjustmentamount = Math.Round(totTax, 4).ToString("0.00");
        //        model.TotalAdjustmentTax = Math.Round(totTax, 4).ToString("0.00");
        //    }
        //    List<string> meterSections = new List<string>();
        //    model.showPayments = depositTransactions.Count() > 0;
        //    model.ShowAdjustments = adjustmentTransactions.Count() > 0;


        //    var groupedTransactions = summaryDto.Transaction.GroupBy(t => t.Meter);
        //    model.ShowGroupedTransactions = groupedTransactions.ToList().Count() > 0;
        //    int rowCount = 0;
        //    double totalUnit = 0;
        //    double totalAmount = 0;
        //    double totaltariff = 0;
        //    foreach (var group in groupedTransactions)
        //    {

        //        if (!group.Key.Contains("ADJUSTMENT", StringComparison.OrdinalIgnoreCase))
        //        {
        //            model.Key = group.Key;



        //            var dayWiseGroups = group
        //                                .GroupBy(t => t.Date.Split(' ')[0])
        //                                .Select(g => new
        //                                {
        //                                    Date = g.Key,
        //                                    Tariff = g.First().Tariff, // pick first or decide how to aggregate
        //                                    TotalUnit = g.Sum(t => Convert.ToDouble(t.Unit)),
        //                                    TotalTaxAmount = g.Sum(t => Convert.ToDouble(t.TaxAmount)),
        //                                    TotalAmount = g.Sum(t => Convert.ToDouble(t.TotalAmount))
        //                                })
        //                                .OrderByDescending(g => DateTime.ParseExact(g.Date, "dd-MM-yyyy", CultureInfo.InvariantCulture))
        //                                .ToList();


        //            foreach (var day in dayWiseGroups)
        //            {


        //                totalUnit += day.TotalTaxAmount;
        //                totalAmount += day.TotalAmount;

        //                meterGroupTransactions.Add(new GroupTransactions
        //                {
        //                    Date = day.Date.Split(' ')[0],
        //                    Tariff = day.Tariff,
        //                    Usage = day.TotalUnit,
        //                    Amount = day.TotalAmount,
        //                    VAT = day.TotalTaxAmount,

        //                });

        //            }
        //            rowCount++;
        //            model.TotalUnit = Math.Round(totalUnit, 4).ToString("0.00");
        //            model.TotalAmount = Math.Round(totalAmount, 4).ToString("0.00");

        //            totalUnit = 0;
        //            totalAmount = 0;
        //        }


        //    }

        //    model.Payments = payments.ToList();
        //    model.Adjustments = adjustments.ToList();
        //    model.MeterTransactions = meterGroupTransactions.ToList();

        //    var template = Template.Parse(txnStmtate.Html);
        //    txnStmtate.Html = template.Render(model, memberRenamer: member => member.Name);

        //    var fullView = new HtmlToPdf();
        //    fullView.Options.WebPageWidth = 1024;
        //    fullView.Options.MinPageLoadTime = 1;
        //    fullView.Options.MaxPageLoadTime = 3;
        //    fullView.Options.WebPageFixedSize = false;
        //    fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

        //    fullView.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
        //    fullView.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;

        //    SelectPdf.PdfDocument doc = fullView.ConvertHtmlString(txnStmtate.Html);

        //    //doc.Save()
        //    byte[] response;
        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            //data.Save(ms);
        //            doc.Save(ms);
        //            response = ms.ToArray();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return response;

        //}


        public async Task DeleteFileAsync(string path, string fileName)
        {
            await Task.Run(() => File.Delete(Path.Combine(path, fileName)));
        }
        /*<td style='text-align:right'><img src='" + relativePath + @"' alt='Logo' style='max-width: 100px; margin-bottom: 10px;'></td>*/

    }
}
