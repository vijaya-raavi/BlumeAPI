using System.Data;
using System.Globalization;
using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Models.Dto.Account;
using Ontec.Core.Domain.Models.Dto.AccountTransactions;
using Ontec.Core.Domain.Models.Dto.Charts;
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

        public GetAccountBalanceByPropertyIdHandler(IPropertyRepository propertyRepository
                                   , IMasterApiConnectService masterApiConnectService
                                   , MasterApiSetting masterApiSetting)
        {
            _propertyRepository = propertyRepository;
            _masterApiConnectService = masterApiConnectService;
            _masterApiSetting = masterApiSetting;

        }
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

                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                if (meterResult != null && string.IsNullOrEmpty(customerAgreementId))
                {
                    customerAgreementId = meterResult.Data[0].CustomerAgreement.Id;
                    balanceDto.AccountName = meterResult.Data[0].CustomerAccount.AccountName;
                    balanceDto.AccountBalance = meterResult.Data[0].CustomerAccount.AccountBalance;
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

            var commonValidator = new GetAccountHisotryQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            AccountBalanceDto balanceDto = new();
           


            var meterList = await _propertyRepository.GetMeterNumbersByPropertyId(request.PropertyId).ConfigureAwait(false);
            string customerAccountId = "";
            string customerAgreementId = "";
            foreach (var meter in meterList)
            {

                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                if (meterResult != null && string.IsNullOrEmpty(customerAccountId))
                {
                    customerAccountId = meterResult.Data[0].CustomerAccount.Id;
                    balanceDto.AccountName = meterResult.Data[0].CustomerAccount.AccountName;
                    customerAgreementId = meterResult.Data[0].CustomerAgreement.Id;
                    balanceDto.AccountBalance= meterResult.Data[0].CustomerAccount.AccountBalance;

                }
            }
            //if (!string.IsNullOrEmpty(customerAgreementId))
            //{
            //    var auxAccountUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxAccountApi + "?filter=(customerAgreementId)(EQ)(" + customerAgreementId
            //                      + ")&paging=(limit)(1)(offset)(0)";
            //    var auxAccountResult = await _masterApiConnectService.GetCustomerAuxAccount(auxAccountUrl).ConfigureAwait(false);
            //    if (auxAccountResult != null)
            //    {
            //        balanceDto.AccountBalance = auxAccountResult.Data[0].Balance;
//
            //    }
            //}
            if (!string.IsNullOrEmpty(customerAccountId))
            {

                var accountTransactionData = new List<AccountTransaction>();
                DateTime endDate = DateTime.UtcNow;
                DateTime startDate = DateTime.UtcNow;

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
                    startDateS += "T00:00:000.000%2B0000";

                    endDate = startDate.AddDays(60);

                    var endDateS = endDate.ToString("yyyy-MM-dd");
                    endDateS += "T00:00:000.000%2B0000";

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
                var transactions = monthlyDepositData .OrderByDescending(t => t.DateEntered).Take(5)
                                    .Select(t => new RececntTransaction
                                    {
                                        TransactionType = t.AccountTransType,
                                        TransactionDate = t.DateEntered,
                                        Amount = t.AmtInclTax,
                                        TransactionId=t.Id
                                       
                                    })
                                    .ToList();
                balanceDto.RececntTransactions= transactions;
            }
            return balanceDto;
        }
    }
}