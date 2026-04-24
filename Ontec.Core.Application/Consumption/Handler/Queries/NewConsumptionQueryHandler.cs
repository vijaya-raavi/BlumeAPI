//using System.Globalization;
//using System.Web.Mvc.Html;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using Ontec.Core.Application.Common.Exceptions;
//using Ontec.Core.Domain.Common;
//using Ontec.Core.Domain.Common.Helper;
//using Ontec.Core.Domain.Enums;
//using Ontec.Core.Domain.Extension;
//using Ontec.Core.Domain.Interface.Common;
//using Ontec.Core.Domain.Interface.Company;
//using Ontec.Core.Domain.Interface.MasterApiService;
//using Ontec.Core.Domain.Interface.Meter;
//using Ontec.Core.Domain.Interface.Property;
//using Ontec.Core.Domain.Interface.User;
//using Ontec.Core.Domain.Models.Dto.Charts;
//using Ontec.Core.Domain.Models.Dto.Common;
//using Ontec.Core.Domain.Models.Dto.Consumption;
//using Ontec.Core.Domain.Requests.Consumption.Queries;

//namespace Ontec.Core.Application.Consumption.Handler.Queries
//{
//    class NewConsumptionQueryHandler : IRequestHandler<GetConsumptionMastersQuery, ConsumptionMastersDto>
//                                           , IRequestHandler<GetConsumptionDashboardQuery, ConsumptionDashboardDto>

//    {
//        private readonly IUserRepository _userRepository;
//        private readonly IMeterRepository _meterRepository;
//        private readonly IPropertyRepository _propertyRepository;
//        private readonly ICompanyRepository _companyRepository;
//        private readonly IWorkContext _workContext;
//        private readonly IMasterApiConnectService _masterApiConnectService;
//        private readonly MasterApiSetting _masterApiSetting;
//        private readonly ILogger<NewConsumptionQueryHandler> _logger;
//        private readonly ICompanyHelper _companyHelper;
//        TimeZoneInfo SaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
//        public NewConsumptionQueryHandler(IUserRepository userRepository
//                                       , IMeterRepository meterRepository
//                                       , ICompanyRepository companyRepository
//                                       , IPropertyRepository propertyRepository
//                                       , IMasterApiConnectService masterApiConnectService
//                                       , MasterApiSetting masterApiSetting
//                                       , IWorkContext workContext
//                                        , ILogger<NewConsumptionQueryHandler> logger
//                                        , ICompanyHelper companyHelper
//            )
//        {
//            _userRepository = userRepository;
//            _propertyRepository = propertyRepository;
//            _companyRepository = companyRepository;
//            _workContext = workContext;
//            _meterRepository = meterRepository;
//            _masterApiConnectService = masterApiConnectService;
//            _masterApiSetting = masterApiSetting;
//            _logger = logger;
//            _companyHelper = companyHelper;

//        }
//        public async Task<ConsumptionMastersDto> Handle(GetConsumptionMastersQuery request, CancellationToken cancellationToken)
//        {
//            request.TrimAllStrings();

//            var commonValidator = new GetConsumptionMastersQueryValidator(_userRepository, _companyRepository, _workContext);
//            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
//            if (!validatorResult.IsValid)
//                throw new ValidationException(validatorResult.Errors);
//            var enumList = EnumHelper.GetSelectList(typeof(ConsumptionCylceEnum));

//            ConsumptionMastersDto masterDto = new();
//            var cycleList = new List<OntecSelectListItem>();
//            foreach (var cycle in enumList.ToList())
//            {
//                cycleList.Add(new OntecSelectListItem
//                {
//                    Id = Int32.Parse(cycle.Value),
//                    Name = cycle.Text
//                });
//            }
//            masterDto.ConsumptionCylceList = cycleList;
//            bool isAdmin = false;

//            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
//            {
//                isAdmin = true;
//            }
//            masterDto.PropertyList = await _propertyRepository.GetConsumptionPropertyList(request.UserId, isAdmin, request.ConsumerId);

//            return masterDto;
//        }
//        public async Task<ConsumptionDashboardDto> Handle(GetConsumptionDashboardQuery request, CancellationToken cancellationToken)
//        {
//            var commonValidator = new GetConsumptionDashboardQueryValidator(_meterRepository);
//            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
//            if (!validatorResult.IsValid)
//                throw new ValidationException(validatorResult.Errors);

//            var targetValueByMeter = await _meterRepository.GetTargetConsumptionByMeterNumber(request.MeterId).ConfigureAwait(false);

//            var dailyConsumption = double.Parse(targetValueByMeter.DailyTargetConsumption);
//            var guageDailyConsumption = dailyConsumption;

//            var enumConsumptionCylce = (ConsumptionCylceEnum)request.ConsumptionCylceTypeId;
//            var consumptions = new List<double>()
//                {
//                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
//                };
//            var XAxisArray = Enumerable.Range(0, 7)
//                                  .Select(offset => DateTime.UtcNow.AddDays(-offset))
//                                  .OrderBy(date => date)
//                                  .Select(date => date.ToString("dd MMM"))
//                                  .ToList();

//            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

//            var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + request.MeterId.ToUpper() + "&paging=(limit)(5)(offset)(0)";

//            var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);

//            DateTime endDate = DateTime.UtcNow.AddDays(1);
//            DateTime startDate = DateTime.UtcNow;

//            switch (request.ConsumptionCylceTypeId)
//            {
//                case 1:
//                    startDate = startDate.AddDays(-6);
//                    break;
//                case 2:
//                    startDate = DateTime.UtcNow.AddDays(-42);
//                    dailyConsumption *= 7;
//                    guageDailyConsumption *= 7;


//                    XAxisArray = Enumerable.Range(0, 42)
//                                .Select(offset => startDate.AddDays(offset))   // 🔥 FIX
//                                .Select(date => ISOWeek.ToDateTime(
//                                    ISOWeek.GetYear(date),
//                                    ISOWeek.GetWeekOfYear(date),
//                                    DayOfWeek.Monday))                          // 🔥 NORMALIZE
//                                .Distinct()
//                                .OrderBy(d => d)
//                                .Select(d => $"{ISOWeek.GetYear(d)}-W{ISOWeek.GetWeekOfYear(d):D2}")
//                                .ToList();

//                    break;
//                case 3:
//                    startDate = DateTime.UtcNow.AddMonths(-12);
//                    dailyConsumption *= 30;
//                    guageDailyConsumption *= 30;
//                    XAxisArray = Enumerable.Range(0, 5)
//                               .Select(offset => DateTime.UtcNow.AddMonths(-offset))
//                              .OrderBy(date => date)
//                               .Select(date => date.ToString("MMM yy"))
//                               .ToList();
//                    break;
//                case 4:
//                    startDate = DateTime.UtcNow.AddYears(-3);
//                    dailyConsumption *= 365;
//                    guageDailyConsumption *= 365;

//                    XAxisArray = Enumerable.Range(0, 3)
//                      .Select(offset => DateTime.UtcNow.AddYears(-offset))
//                      .OrderBy(date => date)
//                      .Select(date => date.Year.ToString())
//                      .ToList();

//                    break;
//            }


//           // DateTime rangeStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
//           // DateTime rangeEnd = rangeStart.AddDays(6);
//            DateTime rangeStart = DateTime.UtcNow;
//            DateTime rangeEnd = DateTime.UtcNow;


//            var blankData = new ConsumptionDashboardDto();
//            if (meterResult != null)
//            {
//                var meterId = meterResult.Data[0].Meter.Id;
//                var meterReadingType = targetValueByMeter.MeterReadingType;// "REAL_ENERGY_FWD";


//                bool isPageEnd = true;
//                var intervalReadingData = new List<Reading>();
//                var intervalDayReadingData = new List<Reading>();
//                var intervalNightReadingData = new List<Reading>();


//                var startDateS = startDate.ToString("yyyy-MM-dd");
//                startDateS += "T00:00:000.000%2B0200";

//                var endDateS = endDate.ToString("yyyy-MM-dd");
//                endDateS += "T00:00:000.000%2B0200";

//                int offsetCount = 0;
//                int pageCount = 5000;
//                bool iterateNext = true;
//                while (iterateNext)
//                {
//                    var inetrvalStart = "&filter=(readingStart)(GTE)(" + startDateS + ")";
//                    var inetrvalEnd = "&filter=(readingEnd)(LTE)(" + endDateS + ")";


//                    var intervalPaging = "&paging=(limit)(" + pageCount + ")(offset)(" + (pageCount * offsetCount) + ")";

//                    var dayIntervalReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;
//                    var dayIntervals = await _masterApiConnectService.GetMeterReadingIntervals(dayIntervalReadingUrl).ConfigureAwait(false);

//                    var solarIntervalReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + "REAL_ENERGY_REV" + inetrvalStart + inetrvalEnd;
//                    var solardayIntervals = await _masterApiConnectService.GetMeterReadingIntervals(dayIntervalReadingUrl).ConfigureAwait(false);


//                    if (dayIntervals?.Data?.Any() == true)
//                    {
//                        intervalReadingData.AddRange(dayIntervals.Data);
//                        offsetCount++;
//                    }
//                    else
//                    {
//                        iterateNext = false;
//                    }
//                }
//                var saZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
//                TimeSpan dayStart = TimeSpan.FromHours(6);
//                TimeSpan dayEnd = TimeSpan.FromHours(18);
//                DateTime monthStart = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0);
//                DateTime monthEnd = monthStart.AddMonths(1); // EXCLUSIVE
//                TimeSpan dayTimeStart = TimeSpan.FromHours(06) + TimeSpan.FromMinutes(0);
//                TimeSpan dayTimeEnd = TimeSpan.FromHours(18) + TimeSpan.FromMinutes(0);

//                TimeSpan nightTimeStart = TimeSpan.FromHours(18) + TimeSpan.FromMinutes(0);
//                TimeSpan nightTimeEnd = TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59);

//                TimeSpan nightTimeStart2 = TimeSpan.FromHours(0) + TimeSpan.FromMinutes(0);
//                TimeSpan nightTimeEnd2 = TimeSpan.FromHours(06) + TimeSpan.FromMinutes(0);

//                var dayReadingTimeStampData = intervalReadingData.Select(a => new
//                {
//                    Original = a,
//                    SATime = a.ReadingStart
//                }).Where(x =>
//                    x.SATime.TimeOfDay >= dayTimeStart &&
//                    x.SATime.TimeOfDay <= dayTimeEnd)
//                                            .GroupBy(x => x.SATime.Date)
//                                            .Select(g => new
//                                            {
//                                                ReadingDate = g.Key,
//                                                ActualConsumption = Math.Round(g.Sum(x => x.Original.ReadingValue) / 1000, 2)
//                                            }).ToList();
//                var nightReadingTimeStampData = intervalReadingData.Select(a => new
//                {
//                    Original = a,
//                    SATime = TimeZoneInfo.ConvertTime(a.ReadingStart, saZone).DateTime
//                }).Where(x => x.SATime.TimeOfDay >= nightTimeStart && x.SATime.TimeOfDay <= nightTimeEnd)
//                  .GroupBy(x => x.SATime.Date).Select(g => new
//                  {
//                      ReadingDate = g.Key,
//                      ActualConsumption = Math.Round(g.Sum(x => x.Original.ReadingValue) / 1000, 2)
//                  }).ToList();

//                var mightReadingTimeStamp2 = intervalReadingData.Select(a => new
//                {
//                    Original = a,
//                    SATime = a.ReadingStart
//                })
//                .Where(x =>
//                    x.SATime.TimeOfDay >= nightTimeStart2 &&
//                    x.SATime.TimeOfDay <= nightTimeEnd2)
//                .GroupBy(x => x.SATime.Date)
//                .Select(g => new
//                {
//                    ReadingDate = g.Key,
//                    ActualConsumption = Math.Round(g.Sum(x => x.Original.ReadingValue) / 1000, 2)
//                })
//                .ToList();

//                nightReadingTimeStampData.AddRange(mightReadingTimeStamp2);

//                intervalReadingData = intervalReadingData.OrderBy(t => t.ReadingTimestamp).ToList();
//                var intervalWise = new List<CaclucationInterval>();
//                var hourlyBase = BuildHourlyBase(intervalReadingData);
//                if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Weekly)
//                {
//                    XAxisArray = hourlyBase
//                    .Select(x => $"{ISOWeek.GetYear(x.Day)}-W{ISOWeek.GetWeekOfYear(x.Day):D2}")
//                    .Distinct()
//                    .OrderBy(x => x)
//                    .ToList();
//                }
//                if (intervalReadingData.Count > 0)
//                {
//                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Daily)
//                    {
//                        //var hourlyBase = BuildHourlyBase(intervalReadingData);
//                        var referenceDay = hourlyBase.Max(x => x.Day);

//                        var isoYear = ISOWeek.GetYear(referenceDay);
//                        var isoWeek = ISOWeek.GetWeekOfYear(referenceDay);

//                        rangeStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
//                        rangeEnd = rangeStart.AddDays(6);
//                        var dailyTotals = hourlyBase
//                            .GroupBy(x => x.Day)
//                            .ToDictionary(
//                                g => g.Key,
//                                g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                            );

//                        intervalWise = new List<CaclucationInterval>();

//                        for (var day = rangeStart; day <= rangeEnd; day = day.AddDays(1))
//                        {
//                            intervalWise.Add(new CaclucationInterval
//                            {
//                                Days = day.ToString("dd MMM"),
//                                ActualConsumption =
//                                    dailyTotals.TryGetValue(day, out var val) ? val : 0
//                            });
//                        }
//                    }



//                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Weekly)
//                    {
//                        intervalWise = hourlyBase
//                              .GroupBy(x => new
//                              {
//                                  IsoYear = ISOWeek.GetYear(x.Day),
//                                  IsoWeek = ISOWeek.GetWeekOfYear(x.Day)
//                              })
//                              .Select(g =>
//                              {
//                                  var weekStart = ISOWeek.ToDateTime(
//                                      g.Key.IsoYear,
//                                      g.Key.IsoWeek,
//                                      DayOfWeek.Monday
//                                  );

//                                  return new CaclucationInterval
//                                  {
//                                      Days = $"{g.Key.IsoYear}-W{g.Key.IsoWeek:D2}",
//                                      StartDate = weekStart,
//                                      EndDate = weekStart.AddDays(7),
//                                      ActualConsumption = Math.Round(
//                                          g.Sum(x => x.HourlyConsumption), 2)
//                                  };
//                              })
//                              .OrderBy(x => x.StartDate)
//                              .ToList();
//                        var dailyTotals = hourlyBase
//                                    .GroupBy(x => x.Day)
//                                    .ToDictionary(
//                                        g => g.Key,
//                                        g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                                    );

//                        var weeks = dailyTotals.Keys
//                            .GroupBy(d => new
//                            {
//                                IsoYear = ISOWeek.GetYear(d),
//                                IsoWeek = ISOWeek.GetWeekOfYear(d)
//                            })
//                            .OrderBy(g => g.Key.IsoYear)
//                            .ThenBy(g => g.Key.IsoWeek);

//                        intervalWise = new List<CaclucationInterval>();

//                        foreach (var week in weeks)
//                        {
//                            var weekStart = ISOWeek.ToDateTime(
//                                week.Key.IsoYear,
//                                week.Key.IsoWeek,
//                                DayOfWeek.Monday
//                            );

//                            double weekTotal = Enumerable.Range(0, 7)
//                                .Sum(i =>
//                                {
//                                    var day = weekStart.AddDays(i);
//                                    return dailyTotals.TryGetValue(day, out var v) ? v : 0;
//                                });

//                            intervalWise.Add(new CaclucationInterval
//                            {
//                                Days = $"{week.Key.IsoYear}-W{week.Key.IsoWeek:D2}",
//                                StartDate = weekStart,
//                                EndDate = weekStart.AddDays(7),
//                                ActualConsumption = Math.Round(weekTotal, 2)
//                            });
//                        }
//                    }


//                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Monthly)
//                    {
//                        intervalWise = hourlyBase
//                            .GroupBy(x => new { x.Day.Year, x.Day.Month })
//                            .OrderBy(g => g.Key.Year)
//                            .ThenBy(g => g.Key.Month)
//                            .Select(g => new CaclucationInterval
//                            {
//                                Days = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
//                                ActualConsumption = Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                            }).ToList();
//                    }
//                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Yearly)
//                    {

//                        intervalWise = hourlyBase
//                           .GroupBy(x => x.Day.Year)
//                            .OrderBy(g => g.Key)
//                            .Select(g => new CaclucationInterval
//                            {
//                                Days = g.Key.ToString(), // "2024"
//                                ActualConsumption = Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                            }).ToList();
//                    }
//                    var xAxisArrayValue = intervalWise.Select(t => t.Days.ToString()).ToList();
//                    consumptions = new List<double>();
//                    foreach (var item in XAxisArray)
//                    {
//                        if (xAxisArrayValue.Contains(item))
//                        {
//                            consumptions.Add(intervalWise.FirstOrDefault(t => t.Days.Equals(item)).ActualConsumption);
//                        }
//                        else
//                            consumptions.Add(0);
//                    }
//                    double actualValue = 0;
//                    var firstRecord = intervalWise.FirstOrDefault();
//                    if (firstRecord != null)
//                    {
//                        actualValue = firstRecord.ActualConsumption;
//                    }
//                    var data = new ConsumptionDashboardDto()
//                    {
//                        AerageConsumption = new AverageConsumption
//                        {
//                            Daily = double.Round(intervalWise.Sum(t => t.ActualConsumption) / XAxisArray.Count(), 2, MidpointRounding.AwayFromZero),
//                            DayTime = double.Round(dayReadingTimeStampData.Sum(t => t.ActualConsumption) / XAxisArray.Count(), 2, MidpointRounding.AwayFromZero),
//                            NightTime = double.Round(nightReadingTimeStampData.Sum(t => t.ActualConsumption) / XAxisArray.Count(), 2, MidpointRounding.AwayFromZero),
//                        },
//                        ConsumptionCycleId = request.ConsumptionCylceTypeId,
//                        ConsumptionCylceType = enumConsumptionCylce.ToString(),
//                        GuageChartDto = new GuageChartDto()
//                        {
//                            ActualValue = actualValue,
//                            TargetValue = guageDailyConsumption
//                        },
//                        MeterUnit = targetValueByMeter.UnitOfMeasure,
//                        LineChartDto = new LineChartDto
//                        {
//                            XAxisdata = XAxisArray,
//                            SeriesLineData = consumptions
//                        }

//                    };
//                    List<ConsumptionDataDTO> result = await GetHourly(request.ConsumptionCylceTypeId, intervalReadingData).ConfigureAwait(false);
//                    data.HourlyData = result;
//                    List<ConsumptionDataDTO> monthlyData = await GetMonthlyConsumptionForYear(request.ConsumptionCylceTypeId, intervalReadingData).ConfigureAwait(false);
//                    data.MonthlyData = monthlyData;
//                    List<ConsumptionDataDTO> weeklyData = await GetWeeklyDataForMonth(request.ConsumptionCylceTypeId, intervalReadingData).ConfigureAwait(false);
//                    data.WeeklyData = weeklyData;
//                    List<ConsumptionDataDTO> dailyData = await GetDaily(request.ConsumptionCylceTypeId, intervalReadingData).ConfigureAwait(false);
//                    data.DailyData = dailyData;



//                    return data;
//                }

//            }
//            consumptions = new List<double>()
//                {
//                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
//                };

//            blankData = new ConsumptionDashboardDto()
//            {
//                AerageConsumption = new AverageConsumption
//                {
//                    Daily = 0,
//                    DayTime = 0,
//                    NightTime = 0,
//                },
//                ConsumptionCycleId = request.ConsumptionCylceTypeId,
//                ConsumptionCylceType = enumConsumptionCylce.ToString(),
//                GuageChartDto = new GuageChartDto()
//                {
//                    ActualValue = 0,
//                    TargetValue = dailyConsumption
//                },
//                MeterUnit = targetValueByMeter.UnitOfMeasure,
//                LineChartDto = new LineChartDto
//                {
//                    XAxisdata = XAxisArray,
//                    SeriesLineData = consumptions
//                }
//            };
//            return blankData;

//        }
//        //private async Task<List<ConsumptionDataDTO>> GetWeeklyDataForMonth(int consumptionCycleId, List<Reading> intervalReadingData)
//        //{
//        //    var data = new List<ConsumptionDataDTO>();

//        //    var hourlyBase = BuildHourlyBase(intervalReadingData);
//        //    hourlyBase = hourlyBase
//        //                .Where(x => x.Day != default)
//        //                .ToList();
//        //    var globalWeeklyTotals = hourlyBase
//        //                            .GroupBy(x => new
//        //                            {
//        //                                IsoYear = ISOWeek.GetYear(x.Day),
//        //                                IsoWeek = ISOWeek.GetWeekOfYear(x.Day)
//        //                            })
//        //                            .ToDictionary(
//        //                                g => $"{g.Key.IsoYear}-W{g.Key.IsoWeek:D2}",
//        //                                g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//        //                            );

//        //    List<DateTime> last12Months = Enumerable.Range(0, 12)
//        //        .Select(offset => DateTime.Today.AddMonths(-offset))
//        //        .OrderBy(d => d)
//        //        .Select(d => new DateTime(d.Year, d.Month, 1))
//        //        .ToList();




//        //    foreach (var monthStart in last12Months)
//        //    {
//        //        var monthEnd = monthStart.AddMonths(1);
//        //        string monthKey = monthStart.ToString("MMM yy");

//        //        var weeklyGroups = hourlyBase
//        //            .GroupBy(x => new
//        //            {
//        //                IsoYear = ISOWeek.GetYear(x.Day),
//        //                IsoWeek = ISOWeek.GetWeekOfYear(x.Day)
//        //            })
//        //            .Select(g =>
//        //            {
//        //                var weekStart = ISOWeek.ToDateTime(
//        //                    g.Key.IsoYear,
//        //                    g.Key.IsoWeek,
//        //                    DayOfWeek.Monday
//        //                );

//        //                return new
//        //                {
//        //                    WeekStart = weekStart,
//        //                    Total = g.Sum(x => x.HourlyConsumption),
//        //                    g.Key.IsoYear,
//        //                    g.Key.IsoWeek
//        //                };
//        //            })
//        //            // ✅ OWNERSHIP: week belongs to month of its ISO Monday
//        //            .Where(w => w.WeekStart >= monthStart && w.WeekStart < monthEnd)
//        //            .OrderBy(w => w.WeekStart)
//        //            .Select(w => new ConsumptionDataDTO
//        //            {
//        //                Year = w.IsoYear,
//        //                Arg = $"{w.IsoYear}-W{w.IsoWeek:D2}",
//        //                StartDate = w.WeekStart,
//        //                EndDate = w.WeekStart.AddDays(7),
//        //                Val = Math.Round(w.Total, 2),
//        //                ParentID = monthKey
//        //            })
//        //            .ToList();

//        //        data.AddRange(weeklyGroups);
//        //    }
//        //    return data;
//        //}

//        //private async Task<List<ConsumptionDataDTO>> GetWeeklyDataForMonth(int consumptionCycleId, List<Reading> intervalReadingData)
//        //{
//        //    var data = new List<ConsumptionDataDTO>();

//        //    var hourlyBase = BuildHourlyBase(intervalReadingData);
//        //    hourlyBase = hourlyBase.Where(x => x.Day != default).ToList();

//        //   // 1.Build daily totals(consistent base)
//        //    var dailyTotals = hourlyBase
//        //        .GroupBy(x => x.Day)
//        //        .ToDictionary(
//        //            g => g.Key,
//        //            g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//        //        );
//        //    var minDate = dailyTotals.Keys.DefaultIfEmpty(DateTime.Today.AddDays(-30)).Min();
//        //    var maxDate = dailyTotals.Keys.DefaultIfEmpty(DateTime.Today).Max();

//        //    var calendarDays = Enumerable.Range(0, (int)(maxDate - minDate).TotalDays)
//        //        .Select(i => minDate.AddDays(i).Date)
//        //        .ToList();

//        //    //var weeks = calendarDays
//        //    //    .GroupBy(d => new
//        //    //    {
//        //    //        IsoYear = ISOWeek.GetYear(d),
//        //    //        IsoWeek = ISOWeek.GetWeekOfYear(d)
//        //    //    })
//        //    //    .OrderBy(g => g.Key.IsoYear).ThenBy(g => g.Key.IsoWeek);
//        //    var weeks = dailyTotals.Keys
//        //        .GroupBy(d => new
//        //        {
//        //            IsoYear = ISOWeek.GetYear(d),
//        //            IsoWeek = ISOWeek.GetWeekOfYear(d)
//        //        })
//        //        .OrderBy(g => g.Key.IsoYear)
//        //        .ThenBy(g => g.Key.IsoWeek);

//        //    List<DateTime> last12Months = Enumerable.Range(0, 12)
//        //        .Select(offset => DateTime.Today.AddMonths(-offset))
//        //        .OrderBy(d => d)
//        //        .Select(d => new DateTime(d.Year, d.Month, 1))
//        //        .ToList();

//        //    foreach (var monthStart in last12Months)
//        //    {
//        //        var monthEnd = monthStart.AddMonths(1);
//        //        string monthKey = monthStart.ToString("MMM yy");

//        //       // 3.Filter weeks whose ISO Monday falls in this month(consistent ownership)
//        //        var monthWeeks = weeks
//        //            .Where(w =>
//        //            {
//        //                var weekStart = ISOWeek.ToDateTime(w.Key.IsoYear, w.Key.IsoWeek, DayOfWeek.Monday);
//        //                return weekStart >= monthStart && weekStart < monthEnd;
//        //            })
//        //            .OrderBy(w => ISOWeek.ToDateTime(w.Key.IsoYear, w.Key.IsoWeek, DayOfWeek.Monday));

//        //        foreach (var week in monthWeeks)
//        //        {
//        //            var weekStart = ISOWeek.ToDateTime(week.Key.IsoYear, week.Key.IsoWeek, DayOfWeek.Monday);
//        //            var weekEnd = weekStart.AddDays(7);
//        //            string weekKey = $"{week.Key.IsoYear}-W{week.Key.IsoWeek:D2}";

//        //            double weekTotal = Enumerable.Range(0, 7)
//        //                .Sum(i =>
//        //                {
//        //                    var day = weekStart.AddDays(i);
//        //                    return dailyTotals.TryGetValue(day, out var v) ? v : 0;
//        //                });

//        //            data.Add(new ConsumptionDataDTO
//        //            {
//        //                Year = week.Key.IsoYear,
//        //                Arg = weekKey,
//        //                StartDate = weekStart,
//        //                EndDate = weekEnd,
//        //                Val = Math.Round(weekTotal, 2),
//        //                ParentID = monthKey
//        //            });
//        //        }
//        //    }
//        //    return data;
//        //}
//        private async Task<List<ConsumptionDataDTO>> GetMonthlyConsumptionForYear(int consumptionCycleId, List<Reading> intervalReadingData)
//        {
//            var data = new List<ConsumptionDataDTO>();
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            var yearly = hourlyBase
//                .GroupBy(x => x.Day.Year)
//                .OrderBy(x => x.Key);

//            foreach (var year in yearly)
//            {
//                double yearTotal = Math.Round(year.Sum(x => x.HourlyConsumption), 2);

//                data.Add(new ConsumptionDataDTO
//                {
//                    Arg = year.Key.ToString(),
//                    Val = yearTotal,
//                    ParentID = ""
//                });

//                var months = year
//                    .GroupBy(x => x.Day.Month)
//                    .OrderBy(x => x.Key);

//                foreach (var month in months)
//                {
//                    string monthKey = new DateTime(year.Key, month.Key, 1)
//                        .ToString("MMM yy");

//                    data.Add(new ConsumptionDataDTO
//                    {
//                        Arg = monthKey,
//                        Val = Math.Round(month.Sum(x => x.HourlyConsumption), 2),
//                        ParentID = year.Key.ToString()
//                    });
//                }
//            }

//            return data;
//        }
//        private async Task<List<ConsumptionDataDTO>> GetWeeklyDataForMonth(int consumptionCycleId, List<Reading> intervalReadingData, string parentMonthKey)
//        {
//            var data = new List<ConsumptionDataDTO>();
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            if (!hourlyBase.Any())
//                return data;

//            // 1️⃣ Daily totals (single source of truth)
//            var dailyTotals = hourlyBase
//                .GroupBy(x => x.Day)
//                .ToDictionary(
//                    g => g.Key,
//                    g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                );

//            var minDate = dailyTotals.Keys.Min();
//            var maxDate = dailyTotals.Keys.Max();

//            // 2️⃣ Loop YEAR → MONTH
//            //for (int year = minDate.Year; year <= maxDate.Year; year++)
//            //{



//            //    for (int month = 1; month <= 12; month++)
//            //    {
//            //        var monthStart = new DateTime(year, month, 1);
//            //        var monthEnd = monthStart.AddMonths(1);

//            //        // Skip months completely outside data range
//            //        if (monthEnd < minDate || monthStart > maxDate)
//            //            continue;

//            //        string monthKey = monthStart.ToString("MMM yy");

//            //        // 3️⃣ Get ALL ISO weeks touching this month
//            //        var weekStarts = Enumerable.Range(0, (monthEnd - monthStart).Days)
//            //            .Select(d => monthStart.AddDays(d))
//            //            .Select(d => ISOWeek.ToDateTime(
//            //                ISOWeek.GetYear(d),
//            //                ISOWeek.GetWeekOfYear(d),
//            //                DayOfWeek.Monday))
//            //            .Distinct()
//            //            .OrderBy(d => d);

//            //        foreach (var weekStart in weekStarts)
//            //        {
//            //            var weekEnd = weekStart.AddDays(7);
//            //            string weekKey =
//            //                $"{ISOWeek.GetYear(weekStart)}-W{ISOWeek.GetWeekOfYear(weekStart):D2}";

//            //            // 4️⃣ Force 7-day calendar sum (missing days → 0)
//            //            double weekTotal = Enumerable.Range(0, 7)
//            //                .Sum(i =>
//            //                {
//            //                    var day = weekStart.AddDays(i);
//            //                    return dailyTotals.TryGetValue(day, out var v) ? v : 0;
//            //                });

//            //            data.Add(new ConsumptionDataDTO
//            //            {
//            //                Year = ISOWeek.GetYear(weekStart),
//            //                Arg = weekKey,
//            //                StartDate = weekStart,
//            //                EndDate = weekEnd,
//            //                Val = Math.Round(weekTotal, 2),
//            //                ParentID = monthKey   // ✅ belongs to month
//            //            });
//            //        }
//            //    }
//            //}

//            for (int year = minDate.Year; year <= maxDate.Year; year++)
//            {
//                for (int month = 1; month <= 12; month++)
//                {
//                    var monthStart = new DateTime(year, month, 1);
//                    var monthEnd = monthStart.AddMonths(1);

//                    //if (monthEnd < minDate || monthStart > maxDate)
//                    //    continue;

//                    string monthKey = monthStart.ToString("MMM yy");


//                    // ✅ ONLY return children for clicked parent
//                    if (!monthKey.Equals(parentMonthKey, StringComparison.OrdinalIgnoreCase))
//                        continue;

//                    var weekStarts = Enumerable.Range(0, (monthEnd - monthStart).Days)
//                        .Select(d => monthStart.AddDays(d))
//                        .Select(d => ISOWeek.ToDateTime(
//                            ISOWeek.GetYear(d),
//                            ISOWeek.GetWeekOfYear(d),
//                            DayOfWeek.Monday))
//                        .Distinct()
//                        .OrderBy(d => d);

//                    foreach (var weekStart in weekStarts)
//                    {
//                        var weekEnd = weekStart.AddDays(7);
//                        string weekKey = $"{ISOWeek.GetYear(weekStart)}-W{ISOWeek.GetWeekOfYear(weekStart):D2}";

//                        double weekTotal = Enumerable.Range(0, 7)
//                            .Sum(i =>
//                            {
//                                var day = weekStart.AddDays(i);
//                                return dailyTotals.TryGetValue(day, out var v) ? v : 0;
//                            });

//                        data.Add(new ConsumptionDataDTO
//                        {
//                            Year = ISOWeek.GetYear(weekStart),
//                            Arg = weekKey,
//                            StartDate = weekStart,
//                            EndDate = weekEnd,
//                            Val = Math.Round(weekTotal, 2),
//                            ParentID = monthKey
//                        });
//                    }
//                }
//            }

//            return data;
//        }
//        private async Task<List<ConsumptionDataDTO>> GetMonthlyConsumptionForYear(int consumptionCycleId, List<Reading> intervalReadingData)
//        {
//            var data = new List<ConsumptionDataDTO>();
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            if (!hourlyBase.Any())
//                return data;

//            var minYear = hourlyBase.Min(x => x.Day.Year);
//            var maxYear = hourlyBase.Max(x => x.Day.Year);

//            for (int year = minYear; year <= maxYear; year++)
//            {
//                string yearKey = year.ToString();

//                double yearTotal = Math.Round(
//                    hourlyBase.Where(x => x.Day.Year == year)
//                              .Sum(x => x.HourlyConsumption), 2);

//                data.Add(new ConsumptionDataDTO
//                {
//                    Arg = yearKey,
//                    Val = yearTotal,
//                    ParentID = ""
//                });

//                for (int month = 1; month <= 12; month++)
//                {
//                    var monthStart = new DateTime(year, month, 1);
//                    var monthEnd = monthStart.AddMonths(1);
//                    string monthKey = monthStart.ToString("MMM yy");

//                    double monthTotal = Math.Round(
//                        hourlyBase
//                            .Where(x => x.Day >= monthStart && x.Day < monthEnd)
//                            .Sum(x => x.HourlyConsumption),
//                        2);

//                    data.Add(new ConsumptionDataDTO
//                    {
//                        Arg = monthKey,
//                        Val = monthTotal,        // ✅ 0 if no data
//                        ParentID = yearKey
//                    });
//                }
//            }

//            return data;
//        }
//        //private async Task<List<ConsumptionDataDTO>> GetDaily(int consumptionCycleTypeId, List<Reading> intervalReadingData)
//        //{
//        //    var data = new List<ConsumptionDataDTO>();
//        //    var hourlyBase = BuildHourlyBase(intervalReadingData);

//        //    // 1️⃣ Daily aggregation
//        //    var dailyTotals = hourlyBase
//        //        .GroupBy(x => x.Day)
//        //        .ToDictionary(
//        //            g => g.Key,
//        //            g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//        //        );

//        //    // 2️⃣ Group by ISO week
//        //    var weeks = dailyTotals.Keys
//        //        .GroupBy(d => new
//        //        {
//        //            IsoYear = ISOWeek.GetYear(d),
//        //            IsoWeek = ISOWeek.GetWeekOfYear(d)
//        //        })
//        //        .OrderBy(g => g.Key.IsoYear)
//        //        .ThenBy(g => g.Key.IsoWeek);

//        //    foreach (var week in weeks)
//        //    {
//        //        // 3️⃣ ISO week boundaries (FIX)
//        //        var weekStart = ISOWeek.ToDateTime(
//        //            week.Key.IsoYear,
//        //            week.Key.IsoWeek,
//        //            DayOfWeek.Monday
//        //        );
//        //        var weekEnd = weekStart.AddDays(7);

//        //        string weekKey = $"{week.Key.IsoYear}-W{week.Key.IsoWeek:D2}";

//        //        // 4️⃣ Add WEEK node
//        //        data.Add(new ConsumptionDataDTO
//        //        {
//        //            Year = week.Key.IsoYear,
//        //            Arg = weekKey,
//        //            StartDate = weekStart,
//        //            EndDate = weekEnd,
//        //            Val = Math.Round(
//        //                Enumerable.Range(0, 7)
//        //                    .Sum(i => dailyTotals.TryGetValue(
//        //                        weekStart.AddDays(i), out var v) ? v : 0),
//        //                2),
//        //            ParentID = ""
//        //        });

//        //        // 5️⃣ Add ALL 7 DAYS (calendar-driven)
//        //        for (int i = 0; i < 7; i++)
//        //        {
//        //            var day = weekStart.AddDays(i);

//        //            data.Add(new ConsumptionDataDTO
//        //            {
//        //                Year = day.Year,
//        //                Arg = day.ToString("dd MMM"),
//        //                StartDate = day,
//        //                EndDate = day.AddDays(1),
//        //                Val = dailyTotals.TryGetValue(day, out var val) ? val : 0,
//        //                ParentID = weekKey
//        //            });
//        //        }
//        //    }

//        //    return data;
//        //}

//        //private async Task<List<ConsumptionDataDTO>> GetDaily(
//        //    int consumptionCycleTypeId,
//        //    List<Reading> intervalReadingData)
//        //{
//        //    var data = new List<ConsumptionDataDTO>();
//        //    var hourlyBase = BuildHourlyBase(intervalReadingData);

//        //    if (!hourlyBase.Any())
//        //        return data;

           
//        //    var referenceDay = hourlyBase.Max(x => x.Day).Date;

           

//        //   int isoYear = ISOWeek.GetYear(referenceDay);
//        //    int isoWeek = ISOWeek.GetWeekOfYear(referenceDay);

//        //    var weekStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
//        //    var weekEnd = weekStart.AddDays(7);

//        //    string weekKey = $"{isoYear}-W{isoWeek:D2}";


//        //   var dailyTotals = hourlyBase
//        //       .GroupBy(x => x.Day.Date)
//        //       .ToDictionary(
//        //           g => g.Key,
//        //           g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//        //       );


//        //   data.Add(new ConsumptionDataDTO
//        //   {
//        //       Arg = weekKey,
//        //       StartDate = weekStart,
//        //       EndDate = weekEnd,
//        //       Val = Math.Round(
//        //           Enumerable.Range(0, 7)
//        //               .Sum(i => dailyTotals.TryGetValue(
//        //                   weekStart.AddDays(i), out var v) ? v : 0),
//        //           2),
//        //       ParentID = ""
//        //   });

//        //    for (int i = 0; i < 7; i++)
//        //        {
//        //            var day = weekStart.AddDays(i);
//        //            var dayKey = day.ToString("dd MMM");

                   
//        //        data.Add(new ConsumptionDataDTO
//        //        {
//        //            Year = day.Year,
//        //            Arg = dayKey,
//        //            StartDate = day,
//        //            EndDate = day.AddDays(1),
//        //            Val = dailyTotals.TryGetValue(day, out var dayVal) ? dayVal : 0,
//        //            ParentID = weekKey
//        //        });

//        //        var dayHours = hourlyBase
//        //            .Where(x => x.Day.Date == day)
//        //            .ToList();

//        //    for (int h = 0; h < 24; h++)
//        //    {
//        //        var hour = dayHours.FirstOrDefault(x => x.Hour == h);

//        //        data.Add(new ConsumptionDataDTO
//        //        {
//        //            Arg = $"{h:00}:00",
//        //            Val = hour?.HourlyConsumption ?? 0,
//        //            ParentID = dayKey
//        //        });
//        //    }
//        //}

//        //    return data;
//        //}
//            private async Task<List<ConsumptionDataDTO>> GetDaily(
//        int consumptionCycleTypeId,
//        List<Reading> intervalReadingData)
//        {
//            var data = new List<ConsumptionDataDTO>();
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            if (!hourlyBase.Any())
//                return data;

//            // 1️⃣ Reference day
//            var referenceDay = hourlyBase.Max(x => x.Day).Date;

//            // 2️⃣ ISO Week
//            int isoYear = ISOWeek.GetYear(referenceDay);
//            int isoWeek = ISOWeek.GetWeekOfYear(referenceDay);

//            var weekStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
//            var weekEnd = weekStart.AddDays(7);

//            string weekKey = $"{isoYear}-W{isoWeek:D2}";

//            // 3️⃣ Daily totals
//            var dailyTotals = hourlyBase
//                .GroupBy(x => x.Day.Date)
//                .ToDictionary(
//                    g => g.Key,
//                    g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                );

//            // 4️⃣ WEEK ROOT
//            data.Add(new ConsumptionDataDTO
//            {
//                Arg = weekKey,
//                StartDate = weekStart,
//                EndDate = weekEnd,
//                Val = Math.Round(
//                    Enumerable.Range(0, 7)
//                        .Sum(i => dailyTotals.TryGetValue(
//                            weekStart.AddDays(i), out var v) ? v : 0),
//                    2),
//                ParentID = ""
//            });

//            // 5️⃣ DAYS + HOURS
//            for (int i = 0; i < 7; i++)
//            {
//                var day = weekStart.AddDays(i);
//                var dayKey = day.ToString("dd MMM");

//                // ✅ DAY NODE (THIS feeds the line chart)
//                data.Add(new ConsumptionDataDTO
//                {
//                    Year = day.Year,
//                    Arg = dayKey,
//                    StartDate = day,
//                    EndDate = day.AddDays(1),
//                    Val = dailyTotals.TryGetValue(day, out var dayVal) ? dayVal : 0,
//                    ParentID = weekKey
//                });

//                // HOURLY CHILDREN
//                var dayHours = hourlyBase
//                    .Where(x => x.Day.Date == day)
//                    .ToList();

//                for (int h = 0; h < 24; h++)
//                {
//                    var hour = dayHours.FirstOrDefault(x => x.Hour == h);

//                    data.Add(new ConsumptionDataDTO
//                    {
//                        Arg = $"{h:00}:00",
//                        Val = hour?.HourlyConsumption ?? 0,
//                        ParentID = dayKey
//                    });
//                }
//            }

//            return data;
//        }
//        private async Task<List<ConsumptionDataDTO>> GetHourly(int consumptionCycleTypeId, List<Reading> intervalReadingData)
//        {
//            var data = new List<ConsumptionDataDTO>();
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            var months = hourlyBase.GroupBy(x => new { x.Day.Year, x.Day.Month }).OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month);

//            foreach (var monthGroup in months)
//            {
//                var monthKey = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1)
//                    .ToString("MMM yy");

//                double monthTotal = Math.Round(monthGroup.Sum(x => x.HourlyConsumption), 2);

//                data.Add(new ConsumptionDataDTO
//                {
//                    Arg = monthKey,
//                    Val = monthTotal,
//                    ParentID = ""
//                });

//                var days = monthGroup.GroupBy(x => x.Day).OrderBy(x => x.Key);

//                foreach (var dayGroup in days)
//                {
//                    string dayKey = dayGroup.Key.ToString("dd MMM");
//                    double dayTotal = Math.Round(dayGroup.Sum(x => x.HourlyConsumption), 2);

//                    data.Add(new ConsumptionDataDTO
//                    {
//                        Arg = dayKey,
//                        StartDate = dayGroup.Key,
//                        EndDate = dayGroup.Key.AddDays(1),
//                        Val = dayTotal,
//                        ParentID = monthKey
//                    });

//                    for (int h = 0; h < 24; h++)
//                    {
//                        var hour = dayGroup.FirstOrDefault(x => x.Hour == h);
//                        data.Add(new ConsumptionDataDTO
//                        {
//                            Arg = $"{h:00}:00",
//                            Val = hour?.HourlyConsumption ?? 0,
//                            ParentID = dayKey
//                        });
//                    }
//                }
//            }

//            return data;
//        }

//        private List<HourlyBucket> BuildHourlyBase(List<Reading> intervalReadingData)
//        {
//            var saZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");

//            return intervalReadingData
//                .Select(r =>
//                {
//                    var saTime = TimeZoneInfo.ConvertTime(r.ReadingStart, saZone).DateTime;

//                    return new
//                    {
//                        Day = saTime.Date,      // ✅ ONLY calendar day
//                        Hour = saTime.Hour,     // ✅ hour
//                        Value = r.ReadingValue / 1000.0
//                    };
//                })
//                .GroupBy(x => new { x.Day, x.Hour }) // ✅ DO NOT include Week/Year
//                .Select(g => new HourlyBucket
//                {
//                    Day = g.Key.Day,
//                    Hour = g.Key.Hour,
//                    HourlyConsumption = Math.Round(g.Sum(x => x.Value), 2)
//                })
//                .ToList();
//        }
//        private int GetWeekOfMonth(DateTime date, DateTime monthStart)
//        {
//            int offset = (int)monthStart.DayOfWeek;
//            return ((date.Day + offset - 1) / 7) + 1;
//        }
//        private static DateTime GetIsoWeekStart(int isoYear, int isoWeek)
//        {
//            return ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
//        }
//        private static DateTime GetIsoWeekStart(DateTime day)
//        {
//            return ISOWeek.ToDateTime(
//                ISOWeek.GetYear(day),
//                ISOWeek.GetWeekOfYear(day),
//                DayOfWeek.Monday
//            );
//        }
//        private List<ConsumptionDataDTO> BuildOneShotHierarchy(
//     List<Reading> intervalReadingData)
//        {
//            var data = new List<ConsumptionDataDTO>();
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            var hourlyMap = hourlyBase
//                .GroupBy(x => (x.Day, x.Hour))
//                .ToDictionary(
//                    g => g.Key,
//                    g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                );

//            var dailyMap = hourlyBase
//                .GroupBy(x => x.Day)
//                .ToDictionary(
//                    g => g.Key,
//                    g => Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                );

//            var minDate = dailyMap.Keys.Min();
//            var maxDate = dailyMap.Keys.Max();

//            for (int year = minDate.Year; year <= maxDate.Year; year++)
//            {
//                string yearKey = year.ToString();

//                data.Add(new ConsumptionDataDTO
//                {
//                    Arg = yearKey,
//                    Val = dailyMap
//                        .Where(d => d.Key.Year == year)
//                        .Sum(d => d.Value),
//                    ParentID = ""
//                });

//                for (int month = 1; month <= 12; month++)
//                {
//                    var monthStart = new DateTime(year, month, 1);
//                    var monthEnd = monthStart.AddMonths(1);

//                    string monthKey = monthStart.ToString("MMM yy");

//                    data.Add(new ConsumptionDataDTO
//                    {
//                        Arg = monthKey,
//                        Val = dailyMap
//                            .Where(d => d.Key >= monthStart && d.Key < monthEnd)
//                            .Sum(d => d.Value),
//                        ParentID = yearKey
//                    });

//                    var weekStarts = Enumerable.Range(0, (monthEnd - monthStart).Days)
//                        .Select(d => monthStart.AddDays(d))
//                        .Select(GetIsoWeekStart)
//                        .Distinct()
//                        .OrderBy(d => d);

//                    foreach (var weekStart in weekStarts)
//                    {
//                        var weekEnd = weekStart.AddDays(7);
//                        string weekKey =
//                            $"{ISOWeek.GetYear(weekStart)}-W{ISOWeek.GetWeekOfYear(weekStart):D2}";

//                        data.Add(new ConsumptionDataDTO
//                        {
//                            Arg = weekKey,
//                            Val = dailyMap
//                                .Where(d => d.Key >= weekStart && d.Key < weekEnd)
//                                .Sum(d => d.Value),
//                            ParentID = monthKey
//                        });

//                        for (int i = 0; i < 7; i++)
//                        {
//                            var day = weekStart.AddDays(i);
//                            string dayKey = day.ToString("dd MMM");

//                            data.Add(new ConsumptionDataDTO
//                            {
//                                Arg = dayKey,
//                                Val = dailyMap.TryGetValue(day, out var v) ? v : 0,
//                                ParentID = weekKey
//                            });

//                            for (int h = 0; h < 24; h++)
//                            {
//                                data.Add(new ConsumptionDataDTO
//                                {
//                                    Arg = $"{h:00}:00",
//                                    Val = hourlyMap.TryGetValue((day, h), out var hv) ? hv : 0,
//                                    ParentID = dayKey
//                                });
//                            }
//                        }
//                    }
//                }
//            }

//            return data;
//        }
//        List<DateTime> GetMonthIsoWeeks(int year, int month)
//        {
//            var start = new DateTime(year, month, 1);
//            var end = start.AddMonths(1).AddDays(-1);

//            var firstMonday = ISOWeek.ToDateTime(
//                ISOWeek.GetYear(start),
//                ISOWeek.GetWeekOfYear(start),
//                DayOfWeek.Monday
//            );

//            var weeks = new List<DateTime>();
//            for (var d = firstMonday; d <= end; d = d.AddDays(7))
//                weeks.Add(d);

//            return weeks;
//        }
//        List<DateTime> GetIsoWeekDays(int year, int week)
//        {
//            var monday = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
//            return Enumerable.Range(0, 7).Select(i => monday.AddDays(i)).ToList();
//        }
//        List<DateTime> GetYearMonths(int year)
//        {
//            return Enumerable.Range(1, 12)
//                .Select(m => new DateTime(year, m, 1))
//                .ToList();
//        }
//        List<int> GetYearAxis(int endYear)
//        {
//            return new List<int>
//                {
//                    endYear - 2,
//                    endYear - 1,
//                    endYear
//                };
//        }
//    }
//}
