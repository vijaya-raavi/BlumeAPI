using System.Data;
using System.Globalization;
using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Models.Dto.Account;
using Ontec.Core.Domain.Models.Dto.AccountTransactions;
using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.MasteUserAccount;
using Ontec.Core.Domain.Requests.Account;
using Ontec.Core.Domain.Requests.Account.Queries;

namespace Ontec.Core.Application.Account.Queries
{
    public class GetAccountBalanceByPropertyIdHandler : IRequestHandler<GetAccountBalanceByPropertyId, AccountBalanceDto>
                                                        , IRequestHandler<GetAccountHisotryQuery, AccountBalanceDto>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly IConfigurationRepository _configurationRepository;

        public GetAccountBalanceByPropertyIdHandler(IPropertyRepository propertyRepository
                                   , IMasterApiConnectService masterApiConnectService
                                   , MasterApiSetting masterApiSetting
                                   , IConfigurationRepository configurationRepository )
        {
            _propertyRepository = propertyRepository;
            _masterApiConnectService = masterApiConnectService;
            _masterApiSetting = masterApiSetting;
            _configurationRepository = configurationRepository;

        }
        //public async Task<AccountBalanceDto> Handle(GetAccountBalanceByPropertyId request, CancellationToken cancellationToken)
        //{
        //    request.TrimAllStrings();

        //    var commonValidator = new GetAccountBalanceByPropertyIdValidator(_propertyRepository);
        //    var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
        //    if (!validatorResult.IsValid)
        //        throw new ValidationException(validatorResult.Errors);

        //    AccountBalanceDto balanceDto = new();

        //    var meterList = await _propertyRepository.GetMeterNumbersByPropertyId(request.PropertyId).ConfigureAwait(false);
        //    string customerAgreementId = "";
        //    foreach (var meter in meterList)
        //    {

        //        var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.ToUpper() + "&paging=(limit)(5)(offset)(0)";
        //        var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
        //        if (meterResult != null && meterResult.Data.Count()>0)
        //        {
        //            customerAgreementId = meterResult.Data[0].CustomerAgreement.Id;
        //            balanceDto.AccountName = meterResult.Data[0].CustomerAccount.AccountName;
        //            balanceDto.AccountBalance = meterResult.Data[0].CustomerAccount.AccountBalance;
        //        }

        //    }
        //    //if (!string.IsNullOrEmpty(customerAgreementId))
        //    //{
        //    //    var auxAccountUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxAccountApi + "?filter=(customerAgreementId)(EQ)(" + customerAgreementId
        //    //                        + ")&paging=(limit)(1)(offset)(0)";
        //    //    var auxAccountResult = await _masterApiConnectService.GetCustomerAuxAccount(auxAccountUrl).ConfigureAwait(false);
        //    //    if (auxAccountResult != null)
        //    //    {
        //    //        balanceDto.AccountBalance = auxAccountResult.Data[0].Balance;

        //    //    }
        //    //}

        //    return balanceDto;
        //}

        //        public async Task<AccountBalanceDto> Handle(GetAccountHisotryQuery request, CancellationToken cancellationToken)
        //        {
        //            request.TrimAllStrings();

        //            var commonValidator = new GetAccountHisotryQueryValidator(_propertyRepository);
        //            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
        //            if (!validatorResult.IsValid)
        //                throw new ValidationException(validatorResult.Errors);

        //            AccountBalanceDto balanceDto = new();



        //            var meterList = await _propertyRepository.GetMeterNumbersByPropertyId(request.PropertyId).ConfigureAwait(false);
        //            string customerAccountId = "";
        //            string customerAgreementId = "";
        //            foreach (var meter in meterList)
        //            {

        //                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.ToUpper() + "&paging=(limit)(5)(offset)(0)";
        //                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
        //                if (meterResult != null && meterResult.Data.Count() > 0)
        //                {
        //                    customerAccountId = meterResult.Data[0].CustomerAccount.Id;
        //                    balanceDto.AccountName = meterResult.Data[0].CustomerAccount.AccountName;
        //                    customerAgreementId = meterResult.Data[0].CustomerAgreement.Id;
        //                    balanceDto.AccountBalance= meterResult.Data[0].CustomerAccount.AccountBalance;

        //                }
        //            }
        //            //if (!string.IsNullOrEmpty(customerAgreementId))
        //            //{
        //            //    var auxAccountUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxAccountApi + "?filter=(customerAgreementId)(EQ)(" + customerAgreementId
        //            //                      + ")&paging=(limit)(1)(offset)(0)";
        //            //    var auxAccountResult = await _masterApiConnectService.GetCustomerAuxAccount(auxAccountUrl).ConfigureAwait(false);
        //            //    if (auxAccountResult != null)
        //            //    {
        //            //        balanceDto.AccountBalance = auxAccountResult.Data[0].Balance;
        ////
        //            //    }
        //            //}
        //            if (!string.IsNullOrEmpty(customerAccountId))
        //            {

        //                var accountTransactionData = new List<AccountTransaction>();
        //                DateTime endDate = DateTime.UtcNow;
        //                DateTime startDate = DateTime.UtcNow;

        //                startDate = DateTime.UtcNow.AddMonths(-4);

        //                var allMonthsInRange = Enumerable.Range(0, 5)
        //                                  .Select(offset => startDate.AddMonths(offset))
        //                                  .Select(date => date.ToString("MMM"))
        //                                  .ToList();

        //                DateTime lastDate = Convert.ToDateTime(startDate);
        //                DateTime currentdate = Convert.ToDateTime(DateTime.UtcNow);
        //                TimeSpan objTimeSpan = currentdate - lastDate;
        //                double days = Convert.ToDouble(objTimeSpan.TotalDays);

        //                Calendar calendar = CultureInfo.CurrentCulture.Calendar;
        //                var monthlyDepositData = new List<AccountTransaction>();
        //                bool isDataEnd = false;
        //                while (!isDataEnd)
        //                {
        //                    //startDate = DateTime.UtcNow.AddDays(-(Math.Round(days) - i));

        //                    var startDateS = startDate.ToString("yyyy-MM-dd");
        //                    startDateS += "T00:00:000.000%2B0000";

        //                    endDate = startDate.AddDays(60);

        //                    var endDateS = endDate.ToString("yyyy-MM-dd");
        //                    endDateS += "T00:00:000.000%2B0000";

        //                    var inetrvalStart = "&filter=(dateEntered)(GTE)(" + startDateS + ")";
        //                    var inetrvalEnd = "&filter=(dateEntered)(LTE)(" + endDateS + ")";

        //                    var paging = "&paging=(limit)(250)(offset)(0)";

        //                    var accountTransactionUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AccountTransApi + "?filter=(customerAccountId)(EQ)(" + customerAccountId + ")" + paging + inetrvalStart + inetrvalEnd + "&filter=(accountTransType)(EQ)(DEPOSIT)";
        //                    var accountTrasactions = await _masterApiConnectService.GetAccountTransactions(accountTransactionUrl).ConfigureAwait(false);

        //                    if (accountTrasactions != null && accountTrasactions.Data.Any())
        //                    {
        //                        monthlyDepositData.AddRange(accountTrasactions.Data);
        //                        startDate = accountTrasactions.Data.Last().DateEntered.AddDays(1);
        //                    }
        //                    else if (startDate <= DateTime.UtcNow)
        //                    {
        //                        startDate = startDate.AddDays(60);
        //                    }
        //                    else
        //                    {
        //                        isDataEnd = true;
        //                    }

        //                }
        //                // var transactions= monthlyDepositData.OrderByDescending(t=>t.DateEntered).Take(5).ToList();
        //                var monthWise = (from month in allMonthsInRange
        //                                 join t in monthlyDepositData
        //                                 on month equals t.DateEntered.ToString("MMM") into gj
        //                                 from sub in gj.DefaultIfEmpty()
        //                                 where sub == null || sub.AccountTransType == "DEPOSIT"
        //                                 group sub by month into g
        //                                 select new
        //                                 {
        //                                     Months = g.Key,
        //                                     AmtInclTax = g.Sum(t => t?.AmtInclTax ?? 0) // Handle null values
        //                                 }).ToList();

        //                var XAxisArray = monthWise.Select(t => t.Months.ToString()).ToList();
        //                var balanceArray = monthWise.Select(t => Convert.ToDouble(t.AmtInclTax)).ToList();
        //                balanceDto.AccountHistory = new LineChartDto
        //                {
        //                    XAxisdata = XAxisArray,
        //                    SeriesLineData = balanceArray
        //                };
        //                var transactions = monthlyDepositData .OrderByDescending(t => t.DateEntered).Take(5)
        //                                    .Select(t => new RececntTransaction
        //                                    {
        //                                        TransactionType = t.AccountTransType,
        //                                        TransactionDate = t.DateEntered,
        //                                        Amount = t.AmtInclTax,
        //                                        TransactionId=t.Id

        //                                    })
        //                                    .ToList();
        //                balanceDto.RececntTransactions= transactions;
        //            }
        //            return balanceDto;
        //        }


        public async Task<AccountBalanceDto> Handle(GetAccountBalanceByPropertyId request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetAccountBalanceByPropertyIdValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            AccountBalanceDto balanceDto = new();

            var meterList = await _propertyRepository.GetMeterNumbersByPropertyId(request.PropertyId).ConfigureAwait(false);
            string customerAgreementId = "";
            foreach (var meter in meterList)
            {

                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                if (meterResult != null && meterResult.Data.Count() > 0)
                {
                    customerAgreementId = meterResult.Data[0].CustomerAgreement.Id;
                    balanceDto.AccountName = meterResult.Data[0].CustomerAccount.AccountName;
                    balanceDto.AccountBalance = meterResult.Data[0].CustomerAccount.AccountBalance;
                    var editableConfiguration = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                    if (editableConfiguration != null)
                    {
                        var config = editableConfiguration.Where(t => t.Name.Contains("auxaccountdetails", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                        if (config != null)
                        {
                            string value = config.Value;
                            if (value == "1")
                            {
                                var auxAccountUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxAccountApi + "?&filter=(customerAgreementId)(EQ)(" + customerAgreementId + ")&paging=(limit)(5)(offset)(0)";


                                var auxAccountResult = await _masterApiConnectService.GetCustomerAuxAccount(auxAccountUrl).ConfigureAwait(false);
                                if (auxAccountResult != null && auxAccountResult.Data.Count() > 0)
                                {
                                    var activeAccounts = auxAccountResult.Data.Where(x => x.RecordStatus == "ACT").ToList();

                                    balanceDto.TotalAuxAccounts = activeAccounts.Count();
                                    balanceDto.AuxAccountBalance = auxAccountResult.Data.Where(x => x.RecordStatus == "ACT").Sum(x => x.Balance);

                                    foreach (var aux in activeAccounts)
                                    {
                                        var auxAccountSummary = new AuxAccountDto
                                        {
                                            Id = aux.Id,
                                            AccountName = aux.AccountName,
                                            Balance = aux.Balance,
                                            AccountPriority = aux.AccountPriority,
                                            RecordStatus = aux.RecordStatus,
                                            SuspendUntil = aux.SuspendUntil,
                                            StartDate = aux.StartDate
                                        };

                                        var id = aux.AuxChargeScheduleId;

                                        if (id > 0)
                                        {
                                            var auxChargeScheduleUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxChargeScheduleApi + "?filter=(id)(EQ)(" + id + ")&paging=(limit)(5)(offset)(0)";

                                            var auxChargeScheduleResult = await _masterApiConnectService.GetCustomerAuxChargeSchedule(auxChargeScheduleUrl).ConfigureAwait(false);

                                            if (auxChargeScheduleResult != null && auxChargeScheduleResult.Data.Count() > 0)
                                            {
                                                if (auxChargeScheduleResult != null && auxChargeScheduleResult.Data?.Count() > 0)
                                                {
                                                    auxAccountSummary.ScheduleCharges = new ChargeScheduleData
                                                    {
                                                        Data = auxChargeScheduleResult.Data.Select(s => new ChargeScheduleDto
                                                        {
                                                            VendPortion = s.VendPortion,
                                                            DailyAmount = s.DailyAmount,
                                                            MinAmt = s.MinAmt,
                                                            MaxAmt = s.MaxAmt
                                                        }).ToList()
                                                    };
                                                }
                                            }


                                            var auxTxneUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AccountTransApi + "?filter=(auxAccountId)(EQ)(" + aux.Id + ")&paging=(limit)(5)(offset)(0)";

                                            var auxAccountTxnResult = await _masterApiConnectService.GetAccountTransactions(auxTxneUrl).ConfigureAwait(false);
                                            if (auxAccountTxnResult != null && auxAccountTxnResult.Data.Count() > 0)
                                            {
                                                auxAccountSummary.Transactions = auxAccountTxnResult.Data.ToList();
                                            }

                                            balanceDto.AuxAccounts.Add(auxAccountSummary);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            //if (!string.IsNullOrEmpty(customerAgreementId))
            //{
            //    var auxAccountUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxAccountApi + "?filter=(customerAgreementId)(EQ)(" + customerAgreementId
            //                        + ")&paging=(limit)(1)(offset)(0)";
            //    var auxAccountResult = await _masterApiConnectService.GetCustomerAuxAccount(auxAccountUrl).ConfigureAwait(false);
            //    if (auxAccountResult != null)
            //    {
            //        balanceDto.AccountBalance = auxAccountResult.Data[0].Balance;

            //    }
            //}

            return balanceDto;
        }

        public async Task<AccountBalanceDto> Handle(GetAccountHisotryQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            DateTime endDate = DateTime.UtcNow;
            DateTime startDate = DateTime.UtcNow;
            var commonValidator = new GetAccountHisotryQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            AccountBalanceDto balanceDto = new();



            var meterList = await _propertyRepository.GetMeterNumbersByPropertyId(request.PropertyId).ConfigureAwait(false);
            if (meterList.ToList().Count() > 0)
            {
                string meterNumber = meterList.FirstOrDefault();
                string customerAccountId = "";
                string customerAgreementId = "";
                string masterMeterId = "";

                // 1. Fetch config ONCE, outside the meter loop
                var editableConfiguration = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                var auxAccountDetailsConfig = editableConfiguration?.FirstOrDefault(t => t.Name.Contains("auxaccountdetails", StringComparison.CurrentCultureIgnoreCase));
                var auxAccountDetailsEnabled = auxAccountDetailsConfig?.Value == "1";

                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meterNumber + "&paging=(limit)(5)(offset)(0)";

                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                if (meterResult.Data != null && meterResult.Data.Count() > 0)
                {
                    var meterData = meterResult.Data[0];
                    customerAccountId = meterData.CustomerAccount.Id;
                    balanceDto.AccountName = meterData.CustomerAccount.AccountName;
                    customerAgreementId = meterData.CustomerAgreement.Id;
                    balanceDto.AccountBalance = meterData.CustomerAccount.AccountBalance;
                    masterMeterId = meterData.Meter.Id;

                    if (auxAccountDetailsEnabled)
                    {
                        var auxAccountUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxAccountApi + "?filter=(customerAgreementId)(EQ)(" + customerAgreementId + ")&paging=(limit)(250)(offset)(0)";

                        var auxAccountResult = await _masterApiConnectService.GetCustomerAuxAccount(auxAccountUrl).ConfigureAwait(false);
                        if (auxAccountResult.Data != null && auxAccountResult.Data.Count() > 0)
                        {

                            var activeAccounts = auxAccountResult.Data.Where(x => x.RecordStatus == "ACT")
                                                                       .DistinctBy(x => x.Id)
                                                                      .OrderBy(x => x.Balance)
                                                                      .ThenBy(x => x.AccountPriority);

                            balanceDto.TotalAuxAccounts = activeAccounts.Count();
                            balanceDto.AuxAccountBalance = activeAccounts.Sum(x => x.Balance);

                            // 2. Parallelize per-account schedule + transaction calls
                            var accountTasks = activeAccounts.Select(async aux =>
                            {
                                var auxAccountSummary = new AuxAccountDto
                                {
                                    Id = aux.Id,
                                    AccountName = aux.AccountName,
                                    Balance = aux.Balance,
                                    AccountPriority = aux.AccountPriority,
                                    RecordStatus = aux.RecordStatus,
                                    SuspendUntil = aux.SuspendUntil,
                                    StartDate = aux.StartDate,
                                    AuxChargeScheduleId = aux.AuxChargeScheduleId

                                };
                                startDate = DateTime.UtcNow.AddMonths(-3);
                                var startDateS = startDate.ToString("yyyy-MM-dd");
                                startDateS += "T00:00:000.000%2B0200";

                                endDate = startDate.AddDays(90);

                                var endDateS = endDate.ToString("yyyy-MM-dd");
                                endDateS += "T00:00:000.000%2B0200";

                                var intervalStart = "&filter=(dateEntered)(GTE)(" + startDateS + ")";
                                var intervalEnd = "&filter=(dateEntered)(LTE)(" + endDateS + ")";
                                var txnUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AccountTransApi + "?filter=(auxAccountId)(EQ)(" + aux.Id + ")" + intervalStart + intervalEnd + "&paging=(limit)(250)(offset)(0)";
                                var txnTask = _masterApiConnectService.GetAccountTransactions(txnUrl);

                                Task<ChargeScheduleData> scheduleTask = null;
                                // replace with actual result type
                                if (aux.AuxChargeScheduleId > 0)
                                {
                                    var scheduleUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxChargeScheduleApi + "?filter=(id)(EQ)(" + aux.AuxChargeScheduleId + ")&paging=(limit)(5)(offset)(0)";
                                    scheduleTask = _masterApiConnectService.GetCustomerAuxChargeSchedule(scheduleUrl);
                                }


                                // run schedule + txn concurrently (schedule may be null/skipped)
                                if (scheduleTask != null)
                                    await Task.WhenAll(scheduleTask, txnTask).ConfigureAwait(false);
                                else
                                    await txnTask.ConfigureAwait(false);

                                if (scheduleTask?.Result?.Data != null && scheduleTask.Result.Data.Any())
                                {
                                    auxAccountSummary.ScheduleCharges = new ChargeScheduleData
                                    {
                                        Data = scheduleTask.Result.Data.Select(s => new ChargeScheduleDto
                                        {
                                            VendPortion = s.VendPortion,
                                            DailyAmount = s.DailyAmount,
                                            MinAmt = s.MinAmt,
                                            MaxAmt = s.MaxAmt,
                                            CurrentPortion = s.CurrentPortion,
                                            ScheduleName = s.ScheduleName,
                                            RecordStatus = s.RecordStatus,
                                            ChargeCycle = s.ChargeCycle,
                                            ChargeAmt = s.ChargeAmt,
                                            AccountSpecific = s.AccountSpecific,
                                        }).ToList()
                                    };
                                }

                                if (txnTask.Result?.Data != null && txnTask.Result.Data.Any())
                                {
                                    auxAccountSummary.Transactions = txnTask.Result.Data.OrderByDescending(t => t.TransDate).ToList();
                                }


                                return auxAccountSummary;
                            });

                            var summaries = await Task.WhenAll(accountTasks).ConfigureAwait(false);
                            balanceDto.AuxAccounts.AddRange(summaries); // every active account now included, per-schedule bug fixed

                        }
                    }
                }

                if (!string.IsNullOrEmpty(customerAccountId))
                {

                    var accountTransactionData = new List<AccountTransaction>();


                    startDate = DateTime.UtcNow.AddMonths(-4);

                    var allMonthsInRange = Enumerable.Range(0, 5)
                                      .Select(offset => startDate.AddMonths(offset))
                                      .Select(date => date.ToString("MMM"))
                                      .ToList();

                    DateTime lastDate = Convert.ToDateTime(startDate);
                    DateTime currentdate = Convert.ToDateTime(DateTime.UtcNow);
                    TimeSpan objTimeSpan = currentdate - lastDate;
                    double days = Convert.ToDouble(objTimeSpan.TotalDays);

                    Calendar calendar = CultureInfo.CurrentCulture.Calendar;
                    var monthlyDepositData = new List<AccountTransaction>();
                    bool isDataEnd = false;
                    while (!isDataEnd)
                    {
                        //startDate = DateTime.UtcNow.AddDays(-(Math.Round(days) - i));

                        var startDateS = startDate.ToString("yyyy-MM-dd");
                        startDateS += "T00:00:000.000%2B0200";

                        endDate = startDate.AddDays(60);

                        var endDateS = endDate.ToString("yyyy-MM-dd");
                        endDateS += "T00:00:000.000%2B0200";

                        var inetrvalStart = "&filter=(dateEntered)(GTE)(" + startDateS + ")";
                        var inetrvalEnd = "&filter=(dateEntered)(LTE)(" + endDateS + ")";

                        var paging = "&paging=(limit)(250)(offset)(0)";

                        var accountTransactionUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AccountTransApi + "?filter=(customerAccountId)(EQ)(" + customerAccountId + ")" + paging + inetrvalStart + inetrvalEnd + "&filter=(accountTransType)(EQ)(DEPOSIT)";
                        var accountTrasactions = await _masterApiConnectService.GetAccountTransactions(accountTransactionUrl).ConfigureAwait(false);

                        if (accountTrasactions != null && accountTrasactions.Data.Any())
                        {
                            monthlyDepositData.AddRange(accountTrasactions.Data);
                            startDate = accountTrasactions.Data.Last().DateEntered.AddDays(1);
                        }
                        else if (startDate <= DateTime.UtcNow)
                        {
                            startDate = startDate.AddDays(60);
                        }
                        else
                        {
                            isDataEnd = true;
                        }

                    }
                    // var transactions= monthlyDepositData.OrderByDescending(t=>t.DateEntered).Take(5).ToList();
                    var monthWise = (from month in allMonthsInRange
                                     join t in monthlyDepositData
                                     on month equals t.DateEntered.ToString("MMM") into gj
                                     from sub in gj.DefaultIfEmpty()
                                     where sub == null || sub.AccountTransType == "DEPOSIT"
                                     group sub by month into g
                                     select new
                                     {
                                         Months = g.Key,
                                         AmtInclTax = g.Sum(t => t?.AmtInclTax ?? 0) // Handle null values
                                     }).ToList();

                    var XAxisArray = monthWise.Select(t => t.Months.ToString()).ToList();
                    var balanceArray = monthWise.Select(t => Convert.ToDouble(t.AmtInclTax)).ToList();
                    balanceDto.AccountHistory = new LineChartDto
                    {
                        XAxisdata = XAxisArray,
                        SeriesLineData = balanceArray
                    };

                    var transactions = monthlyDepositData.OrderByDescending(t => t.DateEntered).Take(5)
                                         .Select(t => new RececntTransaction
                                         {
                                             TransactionType = t.AccountTransType,
                                             TransactionDate = t.TransDate, // already UTC (via JsonSerializerSettings), shift to SAST
                                             Amount = t.AmtInclTax,
                                             TransactionId = t.Id
                                         })
                                         .ToList();
                    balanceDto.RececntTransactions = transactions;

                }
            }
            return balanceDto;
        }
    }
}