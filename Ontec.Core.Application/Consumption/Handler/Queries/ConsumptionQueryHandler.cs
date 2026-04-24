using System.Globalization;
using System.Web.Mvc.Html;
using MediatR;
using Microsoft.Extensions.Logging;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Requests.Consumption.Queries;
using PdfSharpCore.Pdf.IO;

//namespace Ontec.Core.Application.Consumption.Handler.Queries
//{
//    public class ConsumptionQueryHandler : IRequestHandler<GetConsumptionMastersQuery, ConsumptionMastersDto>
//                                           , IRequestHandler<GetConsumptionDashboardQuery, ConsumptionDashboardDto>

//    {
//        private readonly IUserRepository _userRepository;
//        private readonly IMeterRepository _meterRepository;
//        private readonly IPropertyRepository _propertyRepository;
//        private readonly ICompanyRepository _companyRepository;
//        private readonly IWorkContext _workContext;
//        private readonly IMasterApiConnectService _masterApiConnectService;
//        private readonly MasterApiSetting _masterApiSetting;
//        private readonly ILogger<ConsumptionQueryHandler> _logger;
//        private readonly ICompanyHelper _companyHelper;
//        TimeZoneInfo SaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
//        public ConsumptionQueryHandler(IUserRepository userRepository
//                                       , IMeterRepository meterRepository
//                                       , ICompanyRepository companyRepository
//                                       , IPropertyRepository propertyRepository
//                                       , IMasterApiConnectService masterApiConnectService
//                                       , MasterApiSetting masterApiSetting
//                                       , IWorkContext workContext
//                                        , ILogger<ConsumptionQueryHandler> logger
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
//    {
//       0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
//    };
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
//                       .Select(offset => DateTime.UtcNow.AddDays(-offset))
//                       .OrderBy(date => date)
//                       .Select(date => $"{ISOWeek.GetYear(date)}-W{ISOWeek.GetWeekOfYear(date):D2})
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


//                    if (dayIntervals != null)
//                    {
//                        intervalReadingData.AddRange(dayIntervals.Data);
//                        offsetCount++;
//                    }
//                    else
//                    {
//                        iterateNext = false;
//                        break;
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
//                    SATime = a.ReadingTimestamp
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
//                    SATime = a.ReadingTimestamp
//                }).Where(x => x.SATime.TimeOfDay >= nightTimeStart && x.SATime.TimeOfDay <= nightTimeEnd)
//                  .GroupBy(x => x.SATime.Date).Select(g => new
//                  {
//                      ReadingDate = g.Key,
//                      ActualConsumption = Math.Round(g.Sum(x => x.Original.ReadingValue) / 1000, 2)
//                  }).ToList();

//                var mightReadingTimeStamp2 = intervalReadingData.Select(a => new
//                {
//                    Original = a,
//                    SATime = a.ReadingTimestamp
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

//                if (intervalReadingData.Count > 0)
//                {
//                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Daily)
//                    {
//                        intervalWise = intervalReadingData.Select(a => new
//                        {
//                            Original = a,
//                            SATime = TimeZoneInfo.ConvertTime(a.ReadingStart, saZone)
//                        })
//                                        .GroupBy(x => x.SATime.ToString("dd MMM"))
//                                        .Select(g => new CaclucationInterval
//                                        {
//                                            Days = g.Key,
//                                            ActualConsumption = Math.Round(g.Sum(x => x.Original.ReadingValue) / 1000, 2)
//                                        })
//                                        .ToList();
//                    }
//                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Weekly)
//                    {
//                        var hourlyBase = BuildHourlyBase(intervalReadingData);
//                        var monthlyHourly = hourlyBase
//                            .Where(x =>
//                                x.Day >= monthStart &&
//                                x.Day < monthEnd)
//                            .ToList();

//                        intervalWise = monthlyHourly
//                            .GroupBy(x => GetWeekOfMonth(x.Day, monthStart))
//                            .OrderBy(g => g.Key)
//                            .Select(g => new CaclucationInterval
//                            {
//                                Days = "W" + g.Key,   // W1, W2, W3, W4, W5
//                                ActualConsumption = Math.Round(
//                                    g.Sum(x => x.HourlyConsumption), 2)
//                            })
//                            .ToList();
//                    }
//                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Monthly)
//                    {
//                        var hourlyBase = BuildHourlyBase(intervalReadingData);
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
//                        var hourlyBase = BuildHourlyBase(intervalReadingData);

//                        intervalWise = hourlyBase
//                            .GroupBy(x => x.Day.Year)
//                            .OrderBy(g => g.Key)
//                            .Select(g => new CaclucationInterval
//                            {
//                                Days = g.Key.ToString(),
//                                ActualConsumption = Math.Round(
//                                    g.Sum(x => x.HourlyConsumption), 2)
//                            })
//                            .ToList();
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
//    {
//       0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
//    };

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
//        private async Task<List<ConsumptionDataDTO>> GetWeeklyDataForMonth(int consumptionCycleId, List<Reading> intervalReadingData)
//        {
//            var data = new List<ConsumptionDataDTO>();

//            // 1️⃣ Build HOURLY BASE (already proven correct in your system)
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            // 2️⃣ Last 12 months (string keys same as your UI)
//            List<DateTime> last12Months = Enumerable.Range(0, 12)
//                .Select(offset => DateTime.Today.AddMonths(-offset))
//                .OrderBy(d => d)
//                .Select(d => new DateTime(d.Year, d.Month, 1))
//                .ToList();

//            foreach (var monthStart in last12Months)
//            {
//                var monthEnd = monthStart.AddMonths(1);

//                string monthKey = monthStart.ToString("MMM yy");

//                // 3️⃣ FILTER TO MONTH FIRST (CRITICAL)
//                var monthlyHourly = hourlyBase
//                    .Where(x => x.Day >= monthStart && x.Day < monthEnd)
//                    .ToList();

//                if (!monthlyHourly.Any())
//                    continue;

//                // 4️⃣ GROUP BY WEEK-OF-MONTH (NOT week-of-year)
//                var weeklyGroups = monthlyHourly
//                    //.GroupBy(x => GetWeekOfMonth(x.Day, monthStart))
//                    .GroupBy(x => new
//                    {
//                        IsoYear = ISOWeek.GetYear(x.Day),
//                        IsoWeek = ISOWeek.GetWeekOfYear(x.Day)
//                    })
//                    .OrderBy(g => g.Key)
//                    .Select(g => new ConsumptionDataDTO
//                    {
//                        Arg = $"{g.Key.IsoYear}-W{g.Key.IsoWeek:D2}",
//                        Val = Math.Round(g.Sum(x => x.HourlyConsumption), 2),
//                        ParentID = monthKey
//                    })
//                    .ToList();

//                data.AddRange(weeklyGroups);
//            }

//            return data;
//        }
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
//        private async Task<List<ConsumptionDataDTO>> GetDaily(int consumptionCycleTypeId, List<Reading> intervalReadingData)
//        {
//            var data = new List<ConsumptionDataDTO>();
//            var hourlyBase = BuildHourlyBase(intervalReadingData);

//            var daily = hourlyBase
//                .GroupBy(x => x.Day)
//                .Select(g => new
//                {
//                    Day = g.Key,
//                    DailyConsumption = Math.Round(g.Sum(x => x.HourlyConsumption), 2)
//                })
//                .OrderBy(x => x.Day);

//            var calendar = CultureInfo.InvariantCulture.Calendar;
//            var weekRule = CalendarWeekRule.FirstFourDayWeek;
//            var firstDayOfWeek = DayOfWeek.Monday;

//            var weeklyGroups = daily.GroupBy(d => calendar.GetWeekOfYear(d.Day, weekRule, firstDayOfWeek)).OrderBy(g => g.Key);

//            foreach (var week in weeklyGroups)
//            {
//                string weekKey = $"W{week.Key}";
//                double weekTotal = Math.Round(week.Sum(x => x.DailyConsumption), 2);

//                data.Add(new ConsumptionDataDTO
//                {
//                    Arg = weekKey,
//                    Val = weekTotal,
//                    ParentID = ""
//                });

//                foreach (var day in week)
//                {
//                    data.Add(new ConsumptionDataDTO
//                    {
//                        Arg = day.Day.ToString("dd MMM"),
//                        Val = day.DailyConsumption,
//                        ParentID = weekKey
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
//            return intervalReadingData
//                .Select(r => new
//                {
//                    Day = r.ReadingStart.Date,   // ✅ interval START decides day/month
//                    Hour = r.ReadingStart.Hour,
//                    Value = r.ReadingValue / 1000.0
//                }).GroupBy(x => new { x.Day, x.Hour })
//                .Select(g => new HourlyBucket
//                {
//                    Day = g.Key.Day,
//                    Hour = g.Key.Hour,
//                    HourlyConsumption = Math.Round(g.Sum(x => x.Value), 2)
//                }).ToList();
//        }

//        private int GetWeekOfMonth(DateTime date, DateTime monthStart)
//        {
//            int offset = (int)monthStart.DayOfWeek;
//            return ((date.Day + offset - 1) / 7) + 1;
//        }

//    }
//}

