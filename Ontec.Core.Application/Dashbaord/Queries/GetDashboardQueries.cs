using Humanizer;
using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.Services;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.VendRequest;
using Ontec.Core.Domain.Requests.Dashboard;
using Ontec.Core.Domain.Requests.Dashboard.Queries;
using Ontec.Core.Domain.Requests.TopUp.Queries;
using Ontec.Core.Domain.Requests.VendRequest.Commands;

namespace Ontec.Core.Application.Dashbaord.Queries
{
    public class GetDashboardQueries : IRequestHandler<GetAccountBalanceByPropertyIdQuery, PropertyAccountBalanceDto>
                                    , IRequestHandler<GetPropertyDetailsQuery, PropertyDetailsDto>
                                    , IRequestHandler<GetDashboardMastersQuery, IEnumerable<DashboardMastersDto>>
                                    , IRequestHandler<GetDashboardMeterDayConsumptionQuery, IEnumerable<DashboardMeterDayConsumptionDto>>
                                    , IRequestHandler<GetMeterAndUserRequestCount, MeterAndUserRequestDto>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly IUserRepository _userRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly ISignalRService _signalRService;
        private readonly IWorkContext _workContext;
        private readonly ICompanyRepository _companyRepository;
        private readonly IConsumerRepository _consumerRepository;
        private readonly IVendRequestHelper _vendRequestHelper;
        private readonly ITopUpRepository _topUpRepository;
        public GetDashboardQueries(IPropertyRepository propertyRepository
                                    , IMasterApiConnectService masterApiConnectService
                                    , IMeterRepository meterRepository
                                    , MasterApiSetting masterApiSetting
                                    , IUserRepository userRepository
                                    , IWorkContext workContext
                                    , ISignalRService signalRService
                                    , ICompanyRepository companyRepository
                                    , IConsumerRepository consumerRepository
                                    , IVendRequestHelper vendRequestHelper
                                      , ITopUpRepository topUpRepository)
        {
            _propertyRepository = propertyRepository;
            _masterApiConnectService = masterApiConnectService;
            _masterApiSetting = masterApiSetting;
            _meterRepository = meterRepository;
            _userRepository = userRepository;
            _signalRService = signalRService;
            _workContext = workContext;
            _companyRepository = companyRepository;
            _consumerRepository = consumerRepository;
            _vendRequestHelper = vendRequestHelper;
            _topUpRepository = topUpRepository;
        }
        public async Task<PropertyAccountBalanceDto> Handle(GetAccountBalanceByPropertyIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetAccountBalanceByPropertyIdQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            PropertyAccountBalanceDto balanceDto = new();
            var lastToUpDate = DateTime.UtcNow.AddDays(-5).Date.Day.Ordinalize();
            var month = DateTime.UtcNow.ToString("MMM");
            var lastTopUpAmmount = 0;
            balanceDto.LastTopUp = "";
            if (lastTopUpAmmount != 0)
                balanceDto.LastTopUp = "Last top up: R" + lastTopUpAmmount + " on " + lastToUpDate + " " + month;
            var meterList = await _propertyRepository.GetMeterNumbersByPropertyId(request.PropertyId).ConfigureAwait(false);
            string customerAgreementId = "";
            foreach (var meter in meterList)
            {
                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                if (meterResult != null && string.IsNullOrEmpty(customerAgreementId))
                {
                    customerAgreementId = meterResult.Data[0].CustomerAgreement.Id;
                    balanceDto.AccountBalance = meterResult.Data[0].CustomerAccount.AccountBalance;
                }
            }
            // if (!string.IsNullOrEmpty(customerAgreementId))
            // {
            //     var auxAccountUrl = _masterApiSetting.BaseUrl + _masterApiSetting.AuxAccountApi + "?filter=(customerAgreementId)(EQ)(" + customerAgreementId
            //                         + ")&paging=(limit)(1)(offset)(0)";
            //     var auxAccountResult = await _masterApiConnectService.GetCustomerAuxAccount(auxAccountUrl).ConfigureAwait(false);
            //     if (auxAccountResult != null)
            //     {
            //         balanceDto.AccountBalance = auxAccountResult.Data[0].Balance;
            //     }
            // }
            return balanceDto;
        }

        public async Task<PropertyDetailsDto> Handle(GetPropertyDetailsQuery request, CancellationToken cancellationToken)
        {
            var commonValidator = new GetPropertyDetailsQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _propertyRepository.GetPropertyDashboard(request.UserId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<DashboardMastersDto>> Handle(GetDashboardMastersQuery request, CancellationToken cancellationToken)
        {
            var commonValidator = new GetDashboardMastersQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _propertyRepository.GetDashboardMastersDtoByUserId(request.UserId).ConfigureAwait(false);
        }

        //public async Task<IEnumerable<DashboardMeterDayConsumptionDto>> Handle(GetDashboardMeterDayConsumptionQuery request, CancellationToken cancellationToken)
        //{
        //    var commonValidator = new GetDashboardMeterDayConsumptionQueryValidator(_propertyRepository);
        //    var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
        //    if (!validatorResult.IsValid)
        //        throw new ValidationException(validatorResult.Errors);

        //    var meters = await _meterRepository.GetMeterMasterByPropertyId(request.PropertyId).ConfigureAwait(false);

        //    var result = new List<DashboardMeterDayConsumptionDto>();
        //    var solarResult = new SolarConsumptionDTO();
        //    dynamic stsData = new List<GetSTSTopUpTransactions.STSTopUpTransactions>();
        //    var currentMonth = DateTime.Now.Month;
        //    var currentYear = DateTime.Now.Year;
        //    var requestSts = new SendVendSTSRequestCommand();
        //    var resultTxn = new GetSTSTopUpTransactions();
        //    var stsresult = new List<DashboardMeterDayConsumptionDto>();
        //    var stsCurrentMonth = DateTime.Now.Month;
        //    List<string> rctNumbers = new List<string>();
        //    var responses = new List<VendRequestResponse>();
        //    if (meters != null)
        //    {
        //        foreach (var meter in meters)
        //        {
        //            var dailyTargetConsumption = double.Parse(meter.DailyTargetConsumption);

        //            //meter.MeterNumber = "62030884";
        //            meter.GuageChartDto = new GuageChartDto
        //            {
        //                ActualValue = 0,
        //                TargetValue = dailyTargetConsumption
        //            };
        //            meter.DailyTargetConsumption = dailyTargetConsumption.ToString();
        //            var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
        //            var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
        //            if (meterResult != null)
        //            {
        //                var meterId = meterResult.Data[0].Meter.Id;
        //                var installedCapacity = meterResult.Data[0].Meter.PowerLimit;
        //                DateTime endDate = DateTime.UtcNow.AddDays(1);
        //                DateTime startDate = DateTime.UtcNow;
        //                var meterReadingType = meter.MeterReadingType;

        //                var startDateS = startDate.ToString("yyyy-MM-dd");
        //                startDateS += "T00:00:000.000%2B0000";

        //                var endDateS = endDate.ToString("yyyy-MM-dd");
        //                endDateS += "T00:00:000.000%2B0200";

        //                var inetrvalStart = "&filter=(readingStart)(GTE)(" + startDateS + ")";
        //                var inetrvalEnd = "&filter=(readingEnd)(LT)(" + endDateS + ")";

        //                var intervalPaging = "&paging=(limit)(5000)(offset)(0)";


        //                var monthlyConsumption = 0.0;
        //                if (meter.IsSolar)
        //                {
        //                    var solarIntervalReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;

        //                    var solardayIntervals = await _masterApiConnectService.GetMeterReadingIntervals(solarIntervalReadingUrl).ConfigureAwait(false);

        //                    if (solardayIntervals != null)
        //                    {
        //                        var intervalReadings = solardayIntervals.Data;
        //                        var dailyConsumption = solardayIntervals.Data.Sum(t => t.ReadingValue) / 1000;
        //                        meter.GuageChartDto = new GuageChartDto
        //                        {
        //                            ActualValue = dailyConsumption,
        //                            TargetValue = dailyTargetConsumption
        //                        };

        //                        var currentMonthReadings = intervalReadings
        //                        .Where(t => t.ReadingTimestamp.Month == currentMonth && t.ReadingTimestamp.Year == currentYear);

        //                        monthlyConsumption = currentMonthReadings.Sum(t => t.ReadingValue) / 1000;

        //                        solarResult.ExportedToday = dailyConsumption;
        //                        solarResult.ExportedForMonth = monthlyConsumption;

        //                        solarResult.TodayPercentage = (solarResult.ExportedToday / Convert.ToDouble(installedCapacity)) * 100;


        //                    }

        //                    var solarMonthReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;
        //                    var solarMonthIntervals = await _masterApiConnectService.GetMeterReadingIntervals(solarIntervalReadingUrl).ConfigureAwait(false);

        //                    solarResult.InstalledCapacity = Convert.ToDouble(installedCapacity) / 1000;
        //                    meter.SolarData = solarResult;
        //                    solarResult.ExportedForMonth = await GetMonthlySolarData(request.PropertyId);
        //                    solarResult.MeterNumber = meter.MeterNumber.ToUpper();

        //                }
        //                MeterType type = meterResult.Data[0].Meter.Type;
        //                meter.MasterMeterType = type.Name;
        //                //if (type != null && type.Id == "STS" && type.Name == "STS Meter")
        //                //{
        //                //    stsData = await GetSTSTransactionsData(request.PropertyId).ConfigureAwait(false);
        //                //    meter.STSTopUpTransactions = stsData;
        //                //}
        //                if (type != null && type.Id == "STS" && type.Name == "STS Meter")
        //                {

        //                    DateTime firstDateOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        //                    requestSts = new SendVendSTSRequestCommand()
        //                    {

        //                        Meter = meter.MeterNumber.ToUpper(),
        //                        FromDate = firstDateOfMonth,
        //                        ToDate = DateTime.Now,
        //                    };

        //                    responses = (List<VendRequestResponse>)await _vendRequestHelper.ProcessSTSRequest(requestSts).ConfigureAwait(true);
        //                    if (responses.Any())
        //                    {
        //                        foreach (var item in responses)
        //                        {
        //                            if (item.Response != null)
        //                            {
        //                                if (!string.IsNullOrEmpty(item.ReceiptNumber))
        //                                {
        //                                    var isRctNoExist = await _topUpRepository.ISRCTNoExist(item.ReceiptNumber).ConfigureAwait(false);
        //                                    if (isRctNoExist == 0)
        //                                    {
        //                                        var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
        //                                        var meterdt = await _meterRepository.GetMeterByMeterNumber(meter.MeterNumber.ToLower()).ConfigureAwait(false);
        //                                        var property = await _propertyRepository.GetPropertyById(meterdt.PropertyId).ConfigureAwait(false);
        //                                        int userId = 0;
        //                                        int stsMeterId = 0;
        //                                        if (property != null)
        //                                        {
        //                                            userId = property.OwnerId;
        //                                        }
        //                                        if (meter != null)
        //                                        {
        //                                            stsMeterId = meterdt.Id;
        //                                        }

        //                                        var obj = new AddSTSTopUpHelper()
        //                                        {
        //                                            TransactionId = strTransactionNumber,
        //                                            UserId = userId,
        //                                            MeterId = stsMeterId,
        //                                            vendResponse = item.Response,
        //                                            StdToken = item.Token,
        //                                            BsstToken = item.bsstToken,
        //                                            KeyChangeToken = item.keyChangeToken,
        //                                            CustomerMsg = item.customerMsg,
        //                                            MrktMsg = item.mrktMsg,
        //                                            RCTNumber = item.ReceiptNumber,
        //                                            Message = item.Message,
        //                                            TxnDate = item.TxnDatetime
        //                                        };
        //                                        await _topUpRepository.AddTopupTransactionsFromSTSResponse(obj).ConfigureAwait(false);
        //                                        var requstdf = new DownloadPurchaceRecieptPdfQuery()
        //                                        {
        //                                            TransactionId = strTransactionNumber,
        //                                        };

        //                                        if (item.ReceiptNumber != "" && !string.IsNullOrEmpty(item.ReceiptNumber))
        //                                        {
        //                                            rctNumbers.Add(item.ReceiptNumber);
        //                                        }
        //                                        //await _topUpRepository.DownloadPurchaceRecieptPdfQuery(requstdf, userId).ConfigureAwait(false);

        //                                    }
        //                                    else
        //                                    {
        //                                        rctNumbers.Add(item.ReceiptNumber);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    resultTxn = await _topUpRepository.GetSTSTopUps(rctNumbers, requestSts.FromDate.ToUniversalTime(), requestSts.ToDate.ToUniversalTime()).ConfigureAwait(false);
        //                    if (resultTxn.STSTopUpTransaction.Count() > 0)
        //                    {
        //                        stsData = resultTxn;
        //                    }
        //                    else
        //                    {
        //                        stsData = null;
        //                    }
        //                    if (meter != null && stsData != null)
        //                    {
        //                        meter.STSTopUpTransactions = stsData;
        //                    }
        //                }

        //                if (meter != null && !meter.IsSolar)
        //                {
        //                    var dayIntervalReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;
        //                    var dayIntervals = await _masterApiConnectService.GetMeterReadingIntervals(dayIntervalReadingUrl).ConfigureAwait(false);

        //                    if (dayIntervals != null)
        //                    {
        //                        var intervalReadings = dayIntervals.Data;
        //                        var dailyConsumption = dayIntervals.Data.Sum(t => t.ReadingValue) / 1000;
        //                        meter.GuageChartDto = new GuageChartDto
        //                        {
        //                            ActualValue = dailyConsumption,
        //                            TargetValue = dailyTargetConsumption
        //                        };
        //                        meter.Consumption = dailyConsumption.ToString();

        //                    }
        //                }




        //            }



        //            result.Add(meter);


        //        }
        //    }
        //    return result;
        //}


        public async Task<IEnumerable<DashboardMeterDayConsumptionDto>> Handle(GetDashboardMeterDayConsumptionQuery request, CancellationToken cancellationToken)
        {
            var commonValidator = new GetDashboardMeterDayConsumptionQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var meters = await _meterRepository.GetMeterMasterByPropertyId(request.PropertyId).ConfigureAwait(false);

            var result = new List<DashboardMeterDayConsumptionDto>();
            var solarResult = new SolarConsumptionDTO();
            dynamic stsData = new List<GetSTSTopUpTransactions.STSTopUpTransactions>();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var requestSts = new SendVendSTSRequestCommand();
            var resultTxn = new GetSTSTopUpTransactions();
            var stsresult = new List<DashboardMeterDayConsumptionDto>();
            var stsCurrentMonth = DateTime.Now.Month;
            List<string> rctNumbers = new List<string>();
            var responses = new List<VendRequestResponse>();
            if (meters != null)
            {
                foreach (var meter in meters)
                {
                    var dailyTargetConsumption = double.Parse(meter.DailyTargetConsumption);

                    //meter.MeterNumber = "62030884";
                    meter.GuageChartDto = new GuageChartDto
                    {
                        ActualValue = 0,
                        TargetValue = dailyTargetConsumption
                    };
                    meter.DailyTargetConsumption = dailyTargetConsumption.ToString();
                    var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                    var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                    if (meterResult != null && meterResult.Data.Count() > 0)
                    {
                        var meterId = meterResult.Data[0].Meter.Id;
                        var installedCapacity = meterResult.Data[0].Meter.PowerLimit;
                        DateTime endDate = DateTime.UtcNow.AddDays(1);
                        DateTime startDate = DateTime.UtcNow;
                        var meterReadingType = meter.MeterReadingType;

                        var startDateS = startDate.ToString("yyyy-MM-dd");
                        startDateS += "T00:00:000.000%2B0200";

                        var endDateS = endDate.ToString("yyyy-MM-dd");
                        endDateS += "T00:00:000.000%2B0200";

                        var inetrvalStart = "&filter=(readingStart)(GTE)(" + startDateS + ")";
                        var inetrvalEnd = "&filter=(readingEnd)(LT)(" + endDateS + ")";

                        var intervalPaging = "&paging=(limit)(5000)(offset)(0)";


                        var monthlyConsumption = 0.0;
                        if (meter.IsSolar)
                        {
                            var solarIntervalReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;

                            var solardayIntervals = await _masterApiConnectService.GetMeterReadingIntervals(solarIntervalReadingUrl).ConfigureAwait(false);

                            if (solardayIntervals != null)
                            {
                                var intervalReadings = solardayIntervals.Data;
                                var dailyConsumption = solardayIntervals.Data.Sum(t => t.ReadingValue) / 1000;
                                meter.GuageChartDto = new GuageChartDto
                                {
                                    ActualValue = dailyConsumption,
                                    TargetValue = dailyTargetConsumption
                                };

                                var currentMonthReadings = intervalReadings
                                .Where(t => t.ReadingTimestamp.Month == currentMonth && t.ReadingTimestamp.Year == currentYear);

                                monthlyConsumption = currentMonthReadings.Sum(t => t.ReadingValue) / 1000;

                                solarResult.ExportedToday = dailyConsumption;
                                solarResult.ExportedForMonth = monthlyConsumption;

                                solarResult.TodayPercentage = (solarResult.ExportedToday / Convert.ToDouble(installedCapacity)) * 100;


                            }

                            var solarMonthReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;
                            var solarMonthIntervals = await _masterApiConnectService.GetMeterReadingIntervals(solarIntervalReadingUrl).ConfigureAwait(false);

                            solarResult.InstalledCapacity = installedCapacity > 0 ? Convert.ToDouble(installedCapacity) / 1000 : 0;
                            meter.SolarData = solarResult;
                            solarResult.ExportedForMonth = await GetMonthlySolarData(request.PropertyId);
                            solarResult.MeterNumber = meter.MeterNumber.ToUpper();

                        }
                        MeterType type = meterResult.Data[0].Meter.Type;
                        meter.MasterMeterType = type.Name;
                        //if (type != null && type.Id == "STS" && type.Name == "STS Meter")
                        //{
                        //    stsData = await GetSTSTransactionsData(request.PropertyId).ConfigureAwait(false);
                        //    meter.STSTopUpTransactions = stsData;
                        //}
                        if (type != null && type.Id == "STS" && type.Name == "STS Meter")
                        {

                            DateTime firstDateOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                            requestSts = new SendVendSTSRequestCommand()
                            {

                                Meter = meter.MeterNumber.ToUpper(),
                                FromDate = firstDateOfMonth,
                                ToDate = DateTime.Now,
                            };

                            responses = (List<VendRequestResponse>)await _vendRequestHelper.ProcessSTSRequest(requestSts).ConfigureAwait(true);
                            if (responses.Any())
                            {
                                foreach (var item in responses)
                                {
                                    if (item.Response != null)
                                    {
                                        if (!string.IsNullOrEmpty(item.ReceiptNumber))
                                        {
                                            var isRctNoExist = await _topUpRepository.ISRCTNoExist(item.ReceiptNumber).ConfigureAwait(false);
                                            if (isRctNoExist == 0)
                                            {
                                                var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
                                                var meterdt = await _meterRepository.GetMeterByMeterNumber(meter.MeterNumber.ToLower()).ConfigureAwait(false);
                                                var property = await _propertyRepository.GetPropertyById(meterdt.PropertyId).ConfigureAwait(false);
                                                int userId = 0;
                                                int stsMeterId = 0;
                                                if (property != null)
                                                {
                                                    userId = property.OwnerId;
                                                }
                                                if (meter != null)
                                                {
                                                    stsMeterId = meterdt.Id;
                                                }

                                                var obj = new AddSTSTopUpHelper()
                                                {
                                                    TransactionId = strTransactionNumber,
                                                    UserId = userId,
                                                    MeterId = stsMeterId,
                                                    vendResponse = item.Response,
                                                    StdToken = item.Token,
                                                    BsstToken = item.bsstToken,
                                                    KeyChangeToken = item.keyChangeToken,
                                                    CustomerMsg = item.customerMsg,
                                                    MrktMsg = item.mrktMsg,
                                                    RCTNumber = item.ReceiptNumber,
                                                    Message = item.Message,
                                                    TxnDate = item.TxnDatetime
                                                };
                                                await _topUpRepository.AddTopupTransactionsFromSTSResponse(obj).ConfigureAwait(false);
                                                var requstdf = new DownloadPurchaceRecieptPdfQuery()
                                                {
                                                    TransactionId = strTransactionNumber,
                                                };

                                                if (item.ReceiptNumber != "" && !string.IsNullOrEmpty(item.ReceiptNumber))
                                                {
                                                    rctNumbers.Add(item.ReceiptNumber);
                                                }
                                                //await _topUpRepository.DownloadPurchaceRecieptPdfQuery(requstdf, userId).ConfigureAwait(false);

                                            }
                                            else
                                            {
                                                rctNumbers.Add(item.ReceiptNumber);
                                            }
                                        }
                                    }
                                }
                            }
                            resultTxn = await _topUpRepository.GetSTSTopUps(rctNumbers, requestSts.FromDate.ToUniversalTime(), requestSts.ToDate.ToUniversalTime()).ConfigureAwait(false);
                            if (resultTxn.STSTopUpTransaction.Count() > 0)
                            {
                                stsData = resultTxn;
                            }
                            else
                            {
                                stsData = null;
                            }
                            if (meter != null && stsData != null)
                            {
                                meter.STSTopUpTransactions = stsData;
                            }
                        }

                        if (meter != null && !meter.IsSolar)
                        {
                            var dayIntervalReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;
                            var dayIntervals = await _masterApiConnectService.GetMeterReadingIntervals(dayIntervalReadingUrl).ConfigureAwait(false);

                            if (dayIntervals != null)
                            {
                                var intervalReadings = dayIntervals.Data;
                                var dailyConsumption = dayIntervals.Data.Sum(t => t.ReadingValue) / 1000;
                                meter.GuageChartDto = new GuageChartDto
                                {
                                    ActualValue = dailyConsumption,
                                    TargetValue = dailyTargetConsumption
                                };
                                meter.Consumption = dailyConsumption.ToString();

                            }
                        }




                    }



                    result.Add(meter);


                }
            }
            return result;
        }

        public async Task<MeterAndUserRequestDto> Handle(GetMeterAndUserRequestCount request, CancellationToken cancellationToken)
        {
            return await _consumerRepository.GetMeterAndUserRequestCount().ConfigureAwait(false);
        }

        private async Task<double> GetMonthlySolarData(int propertyId)
        {

            var meters = await _meterRepository.GetMeterMasterByPropertyId(propertyId).ConfigureAwait(false);
            var result = new List<DashboardMeterDayConsumptionDto>();
            var solarResult = new SolarConsumptionDTO();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            DateTime now = DateTime.UtcNow;
            var monthStartDate = new DateTime(now.Year, now.Month, 1);
            var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
            double monthlyExported = 0.0;
            if (meters != null)
            {
                foreach (var meter in meters)
                {
                    var dailyTargetConsumption = double.Parse(meter.DailyTargetConsumption);

                    //meter.MeterNumber = "62030884";
                    meter.GuageChartDto = new GuageChartDto
                    {
                        ActualValue = 0,
                        TargetValue = dailyTargetConsumption
                    };
                    meter.DailyTargetConsumption = dailyTargetConsumption.ToString();
                    var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                    var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                    if (meterResult != null)
                    {
                        var meterId = meterResult.Data[0].Meter.Id;
                        var installedCapacity = meterResult.Data[0].Meter.PowerLimit;
                        DateTime endDate = monthEndDate.AddDays(1);
                        DateTime startDate = monthStartDate;
                        var meterReadingType = meter.MeterReadingType;

                        var startDateS = startDate.ToString("yyyy-MM-dd");
                        startDateS += "T00:00:000.000%2B0200";

                        var endDateS = endDate.ToString("yyyy-MM-dd");
                        endDateS += "T00:00:000.000%2B0200";

                        var inetrvalStart = "&filter=(readingStart)(GTE)(" + startDateS + ")";
                        var inetrvalEnd = "&filter=(readingEnd)(LT)(" + endDateS + ")";
                        var intervalPaging = "&paging=(limit)(5000)(offset)(0)";
                        var monthlyConsumption = 0.0;
                        if (meter.IsSolar)
                        {
                            var solarMonthReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;
                            var solarMonthIntervals = await _masterApiConnectService.GetMeterReadingIntervals(solarMonthReadingUrl).ConfigureAwait(false);
                            if (solarMonthIntervals != null)
                            {
                                var intervalReadings = solarMonthIntervals.Data;
                                var dailyConsumption = solarMonthIntervals.Data.Sum(t => t.ReadingValue) / 1000;
                                meter.GuageChartDto = new GuageChartDto
                                {
                                    ActualValue = dailyConsumption,
                                    TargetValue = dailyTargetConsumption
                                };
                                var currentMonthReadings = intervalReadings
                                .Where(t => t.ReadingTimestamp.Month == currentMonth && t.ReadingTimestamp.Year == currentYear);
                                monthlyExported = currentMonthReadings.Sum(t => t.ReadingValue) / 1000;
                            }
                        }
                    }
                }
            }
            return monthlyExported;
        }


        //private async Task<double> GetMonthlySolarData(int propertyId)
        //{

        //    var meters = await _meterRepository.GetMeterMasterByPropertyId(propertyId).ConfigureAwait(false);
        //    var result = new List<DashboardMeterDayConsumptionDto>();
        //    var solarResult = new SolarConsumptionDTO();
        //    var currentMonth = DateTime.Now.Month;
        //    var currentYear = DateTime.Now.Year;
        //    DateTime now = DateTime.UtcNow;
        //    var monthStartDate = new DateTime(now.Year, now.Month, 1);
        //    var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
        //    double monthlyExported = 0.0;
        //    if (meters != null)
        //    {
        //        foreach (var meter in meters)
        //        {
        //            var dailyTargetConsumption = double.Parse(meter.DailyTargetConsumption);

        //            //meter.MeterNumber = "62030884";
        //            meter.GuageChartDto = new GuageChartDto
        //            {
        //                ActualValue = 0,
        //                TargetValue = dailyTargetConsumption
        //            };
        //            meter.DailyTargetConsumption = dailyTargetConsumption.ToString();
        //            var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
        //            var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
        //            if (meterResult != null && meterResult.Data.Count() > 0)
        //            {
        //                var meterId = meterResult.Data[0].Meter.Id;
        //                var installedCapacity = meterResult.Data[0].Meter.PowerLimit;
        //                DateTime endDate = monthEndDate.AddDays(1);
        //                DateTime startDate = monthStartDate;
        //                var meterReadingType = meter.MeterReadingType;

        //                var startDateS = startDate.ToString("yyyy-MM-dd");
        //                startDateS += "T00:00:000.000%2B0200";

        //                var endDateS = endDate.ToString("yyyy-MM-dd");
        //                endDateS += "T00:00:000.000%2B0200";

        //                var inetrvalStart = "&filter=(readingStart)(GTE)(" + startDateS + ")";
        //                var inetrvalEnd = "&filter=(readingEnd)(LT)(" + endDateS + ")";
        //                var intervalPaging = "&paging=(limit)(5000)(offset)(0)";
        //                var monthlyConsumption = 0.0;
        //                if (meter.IsSolar)
        //                {
        //                    var solarMonthReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;
        //                    var solarMonthIntervals = await _masterApiConnectService.GetMeterReadingIntervals(solarMonthReadingUrl).ConfigureAwait(false);
        //                    if (solarMonthIntervals != null)
        //                    {
        //                        var intervalReadings = solarMonthIntervals.Data;
        //                        var dailyConsumption = solarMonthIntervals.Data.Sum(t => t.ReadingValue) / 1000;
        //                        meter.GuageChartDto = new GuageChartDto
        //                        {
        //                            ActualValue = dailyConsumption,
        //                            TargetValue = dailyTargetConsumption
        //                        };
        //                        var currentMonthReadings = intervalReadings
        //                        .Where(t => t.ReadingTimestamp.Month == currentMonth && t.ReadingTimestamp.Year == currentYear);
        //                        monthlyExported = currentMonthReadings.Sum(t => t.ReadingValue) / 1000;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return monthlyExported;
        //}
        private async Task<GetSTSTopUpTransactions> GetSTSTransactionsData(int propertyId)
        {
            var requestSts = new SendVendSTSRequestCommand();
            var resultTxn = new GetSTSTopUpTransactions();
            var meters = await _meterRepository.GetMeterMasterByPropertyId(propertyId).ConfigureAwait(false);
            var result = new List<DashboardMeterDayConsumptionDto>();
            var currentMonth = DateTime.Now.Month;
            List<string> rctNumbers = new List<string>();
            var responses = new List<VendRequestResponse>();
            DateTime firstDateOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (meters != null)
            {
                foreach (var meter in meters)
                {
                    if (meter != null)
                    {

                        requestSts = new SendVendSTSRequestCommand()
                        {

                            Meter = meter.MeterNumber.ToUpper(),
                            FromDate = firstDateOfMonth,
                            ToDate = DateTime.Now,
                        };

                        responses = (List<VendRequestResponse>)await _vendRequestHelper.ProcessSTSRequest(requestSts).ConfigureAwait(true);


                        if (responses.Any())
                        {
                            foreach (var item in responses)
                            {
                                if (item.Response != null)
                                {
                                    if (!string.IsNullOrEmpty(item.ReceiptNumber))
                                    {
                                        var isRctNoExist = await _topUpRepository.ISRCTNoExist(item.ReceiptNumber).ConfigureAwait(false);
                                        if (isRctNoExist == 0)
                                        {
                                            var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
                                            var meterdt = await _meterRepository.GetMeterByMeterNumber(meter.MeterNumber.ToLower()).ConfigureAwait(false);
                                            var property = await _propertyRepository.GetPropertyById(meterdt.PropertyId).ConfigureAwait(false);
                                            int userId = 0;
                                            int meterId = 0;
                                            if (property != null)
                                            {
                                                userId = property.OwnerId;
                                            }
                                            if (meter != null)
                                            {
                                                meterId = meterdt.Id;
                                            }

                                            var obj = new AddSTSTopUpHelper()
                                            {
                                                TransactionId = strTransactionNumber,
                                                UserId = userId,
                                                MeterId = meterId,
                                                vendResponse = item.Response,
                                                StdToken = item.Token,
                                                BsstToken = item.bsstToken,
                                                KeyChangeToken = item.keyChangeToken,
                                                CustomerMsg = item.customerMsg,
                                                MrktMsg = item.mrktMsg,
                                                RCTNumber = item.ReceiptNumber,
                                                Message = item.Message,
                                                TxnDate = item.TxnDatetime
                                            };
                                            await _topUpRepository.AddTopupTransactionsFromSTSResponse(obj).ConfigureAwait(false);
                                            var requstdf = new DownloadPurchaceRecieptPdfQuery()
                                            {
                                                TransactionId = strTransactionNumber,
                                            };

                                            if (item.ReceiptNumber != "" && !string.IsNullOrEmpty(item.ReceiptNumber))
                                            {
                                                rctNumbers.Add(item.ReceiptNumber);
                                            }
                                            //await _topUpRepository.DownloadPurchaceRecieptPdfQuery(requstdf, userId).ConfigureAwait(false);

                                        }
                                        else
                                        {
                                            rctNumbers.Add(item.ReceiptNumber);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            resultTxn = await _topUpRepository.GetSTSTopUps(rctNumbers, requestSts.FromDate.ToUniversalTime(), requestSts.ToDate.ToUniversalTime()).ConfigureAwait(false);
            return resultTxn;
        }



    }
}
