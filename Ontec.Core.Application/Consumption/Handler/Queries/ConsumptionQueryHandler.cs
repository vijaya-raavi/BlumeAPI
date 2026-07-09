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

namespace Ontec.Core.Application.Consumption.Handler.Queries
{
    public class ConsumptionQueryHandler : IRequestHandler<GetConsumptionMastersQuery, ConsumptionMastersDto>
                                           , IRequestHandler<GetConsumptionDashboardQuery, ConsumptionDashboardDto>

    {
        private readonly IUserRepository _userRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IWorkContext _workContext;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly ILogger<ConsumptionQueryHandler> _logger;
        private readonly ICompanyHelper _companyHelper;
        TimeZoneInfo SaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
        public ConsumptionQueryHandler(IUserRepository userRepository
                                       , IMeterRepository meterRepository
                                       , ICompanyRepository companyRepository
                                       , IPropertyRepository propertyRepository
                                       , IMasterApiConnectService masterApiConnectService
                                       , MasterApiSetting masterApiSetting
                                       , IWorkContext workContext
                                        , ILogger<ConsumptionQueryHandler> logger
                                        , ICompanyHelper companyHelper
            )
        {
            _userRepository = userRepository;
            _propertyRepository = propertyRepository;
            _companyRepository = companyRepository;
            _workContext = workContext;
            _meterRepository = meterRepository;
            _masterApiConnectService = masterApiConnectService;
            _masterApiSetting = masterApiSetting;
            _logger = logger;
            _companyHelper = companyHelper;

        }
        public async Task<ConsumptionMastersDto> Handle(GetConsumptionMastersQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetConsumptionMastersQueryValidator(_userRepository, _companyRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var enumList = EnumHelper.GetSelectList(typeof(ConsumptionCylceEnum));

            ConsumptionMastersDto masterDto = new();
            var cycleList = new List<OntecSelectListItem>();
            foreach (var cycle in enumList.ToList())
            {
                cycleList.Add(new OntecSelectListItem
                {
                    Id = Int32.Parse(cycle.Value),
                    Name = cycle.Text
                });
            }
            masterDto.ConsumptionCylceList = cycleList;
            bool isAdmin = false;

            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                isAdmin = true;
            }
            masterDto.PropertyList = await _propertyRepository.GetConsumptionPropertyList(request.UserId, isAdmin, request.ConsumerId);

            return masterDto;
        }

        private string GetMonthWeekId(DateTime date)
        {
            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var weekStart = monthStart;
            int weekCounter = 1;

            while (weekStart <= monthEnd)
            {
                var weekEnd = weekStart.AddDays(6 - (int)weekStart.DayOfWeek);

                if (weekEnd > monthEnd)
                    weekEnd = monthEnd;

                if (date >= weekStart && date <= weekEnd)
                {
                    return $"{date:yyyy-MM}-W{weekCounter}";
                }

                weekStart = weekEnd.AddDays(1);
                weekCounter++;
            }

            return $"{date:yyyy-MM}-W1";
        }
        public async Task<ConsumptionDashboardDto> Handle(GetConsumptionDashboardQuery request, CancellationToken cancellationToken)
        {
            var commonValidator = new GetConsumptionDashboardQueryValidator(_meterRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var targetValueByMeter = await _meterRepository.GetTargetConsumptionByMeterNumber(request.MeterId).ConfigureAwait(false);
            var companyDetails = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
            var dailyConsumption = targetValueByMeter.DailyTargetConsumption;
            var guageDailyConsumption = dailyConsumption;

            var enumConsumptionCylce = (ConsumptionCylceEnum)request.ConsumptionCylceTypeId;
            var consumptions = new List<double>()
                    {
                       0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
                    };
            var XAxisArray = Enumerable.Range(0, 7)
                  .Select(offset => DateTime.UtcNow.AddDays(-offset))
                  .OrderBy(date => date)
                  .Select(date => date.ToString("dd MMM"))
                  .ToList();

            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

            var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + request.MeterId.ToUpper() + "&paging=(limit)(5)(offset)(0)";

            var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);

            //correct one
            //DateTime endDate = DateTime.UtcNow.
            //AddDays(1);
            var today = DateTime.UtcNow.Date;
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = startDate.AddDays(7);//for a test
            switch (request.ConsumptionCylceTypeId)
            {
                case 1:
                    startDate = startDate.AddDays(-6);
                    endDate = DateTime.UtcNow;
                    break;
                case 2:


                    var currentWeekStart = today.AddDays(-(int)today.DayOfWeek);
                    startDate = currentWeekStart.AddDays(-6 * 7);
                    endDate = DateTime.UtcNow;
                    guageDailyConsumption *= 7;
                    XAxisArray = Enumerable.Range(0, 7)
                        .Select(i => startDate.AddDays(i * 7))
                        .Select(weekStart =>
                            $"{weekStart:dd MMM} - {weekStart.AddDays(6):dd MMM}")
                        .ToList();

                    break;
                case 3:
                    startDate = DateTime.UtcNow.AddMonths(-5);
                    dailyConsumption *= 30;

                    int daysInCurrentMonth = DateTime.DaysInMonth(
                                        DateTime.UtcNow.Year,
                                        DateTime.UtcNow.Month);
                    guageDailyConsumption *= daysInCurrentMonth;
                    XAxisArray = Enumerable.Range(0, 5)
                               .Select(offset => DateTime.UtcNow.AddMonths(-offset))
                              .OrderBy(date => date)
                               .Select(date => date.ToString("MMM yy"))
                               .ToList();
                    endDate = DateTime.UtcNow;
                    break;
                case 4:
                    startDate = DateTime.UtcNow.AddYears(-3);
                    dailyConsumption *= DateTime.IsLeapYear(startDate.Year) ? 366 : 365;
                    guageDailyConsumption *= DateTime.IsLeapYear(startDate.Year) ? 366 : 365;

                    XAxisArray = Enumerable.Range(0, 3)
                      .Select(offset => DateTime.UtcNow.AddYears(-offset))
                      .OrderBy(date => date)
                      .Select(date => date.Year.ToString())
                      .ToList();
                    endDate = DateTime.UtcNow;
                    break;
            }
            var blankData = new ConsumptionDashboardDto();
            if (meterResult != null)
            {
                var meterId = meterResult.Data[0].Meter.Id;
                var meterReadingType = targetValueByMeter.MeterReadingType;// "REAL_ENERGY_FWD";


                bool isPageEnd = true;
                var intervalReadingData = new List<Reading>();
                var intervalDayReadingData = new List<Reading>();
                var intervalNightReadingData = new List<Reading>();

                //correct one
                var startDateS = startDate.ToString("yyyy-MM-dd");
                startDateS += "T00:00:000.000%2B0200";

                var endDateS = endDate.ToString("yyyy-MM-dd");
                endDateS += "T23:59:599.999%2B0200";


                int offsetCount = 0;
                int pageCount = 5000;
                bool iterateNext = true;


                while (true)
                {
                    var inetrvalStart = "&filter=(readingStart)(GTE)(" + startDateS + ")";
                    var inetrvalEnd = "&filter=(readingStart)(LT)(" + endDateS + ")";


                    var intervalPaging = "&paging=(limit)(" + pageCount + ")(offset)(" + (pageCount * offsetCount) + ")";

                    var dayIntervalReadingUrl = _masterApiSetting.BaseUrl + _masterApiSetting.IntervalReadingApi + "?meterId=" + meterId + intervalPaging + "&meterReadingType=" + meterReadingType + inetrvalStart + inetrvalEnd;

                    var dayIntervals = await _masterApiConnectService.GetMeterReadingIntervals(dayIntervalReadingUrl).ConfigureAwait(false);

                    if (dayIntervals?.Data == null || dayIntervals.Data.Count == 0)
                        break;

                    intervalReadingData.AddRange(dayIntervals.Data);

                    if (dayIntervals.Data.Count < pageCount)
                        break;

                    offsetCount++;
                }

                var saZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
                intervalReadingData = intervalReadingData
                                    .Select(r =>
                                    {
                                        r.ReadingStart = TimeZoneInfo.ConvertTime(r.ReadingStart, saZone);
                                        r.ReadingEnd = TimeZoneInfo.ConvertTime(r.ReadingEnd, saZone);
                                        return r;
                                    })
                                    .OrderBy(r => r.ReadingStart)
                                    .ToList();


                TimeSpan dayStart = TimeSpan.FromHours(6);
                TimeSpan dayEnd = TimeSpan.FromHours(18);
                DateTime monthStart = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0);
                DateTime monthEnd = monthStart.AddMonths(1); // EXCLUSIVE
                TimeSpan dayTimeStart = TimeSpan.FromHours(06) + TimeSpan.FromMinutes(0);
                TimeSpan dayTimeEnd = TimeSpan.FromHours(18) + TimeSpan.FromMinutes(0);

                TimeSpan nightTimeStart = TimeSpan.FromHours(18) + TimeSpan.FromMinutes(0);
                TimeSpan nightTimeEnd = TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59);

                TimeSpan nightTimeStart2 = TimeSpan.FromHours(0) + TimeSpan.FromMinutes(0);
                TimeSpan nightTimeEnd2 = TimeSpan.FromHours(06) + TimeSpan.FromMinutes(0);
                double avgPerDay = 0;
                double avgPerNight = 0;
                double dtotal = 0;
                double ntotal = 0;

                int totalDays = 0;
                var lastDate = DateTime.UtcNow.Date;

                var nightGroupedData = intervalReadingData
                                         .Where(r =>
                                             (
                                                 r.ReadingStart.TimeOfDay >= nightTimeStart &&
                                                 r.ReadingStart.TimeOfDay <= nightTimeEnd
                                             )
                                             ||
                                             (
                                                 r.ReadingStart.TimeOfDay >= nightTimeStart2 &&
                                                 r.ReadingStart.TimeOfDay < nightTimeEnd2
                                             )
                                         )
                                         .GroupBy(r =>
                                             new DateTime(
                                                 r.ReadingStart.Year,
                                                 r.ReadingStart.Month,
                                                 1))
                                         .ToDictionary(
                                             g => g.Key,
                                             g => Math.Round(
                                                 g.Sum(x => x.ReadingValue) / 1000.0,
                                                 2)
                                         );
                var dayGroupedData = intervalReadingData
                                        .Where(r =>
                                            r.ReadingStart.TimeOfDay >= dayTimeStart &&
                                            r.ReadingStart.TimeOfDay < dayTimeEnd)
                                        .GroupBy(r =>
                                            new DateTime(
                                                r.ReadingStart.Year,
                                                r.ReadingStart.Month,
                                                1))
                                        .ToDictionary(
                                            g => g.Key,
                                            g => Math.Round(g.Sum(x => x.ReadingValue) / 1000.0, 2)
                                        );

                dayGroupedData = dayGroupedData
                            .Where(x => x.Key >= startDate && x.Key <= endDate)
                            .ToDictionary(x => x.Key, x => x.Value);
                dtotal = dayGroupedData.
                           Where(x => x.Key >= startDate && x.Key <= endDate)
                          .Sum(x => x.Value);




                nightGroupedData = nightGroupedData
                            .Where(x => x.Key >= startDate && x.Key <= endDate)
                            .ToDictionary(x => x.Key, x => x.Value);

                ntotal = nightGroupedData.
                           Where(x => x.Key >= startDate && x.Key <= endDate)
                          .Sum(x => x.Value);
                var dayIntervalWise = new List<CaclucationInterval>();
                var nightIntervalWise = new List<CaclucationInterval>();
                intervalReadingData = intervalReadingData.OrderBy(t => t.ReadingTimestamp).ToList();
                var intervalWise = new List<CaclucationInterval>();

                if (intervalReadingData.Count > 0)
                {
                    var firstDate = intervalReadingData.Min(x => x.ReadingStart.Date);
                    var selMonthStart = new DateTime(startDate.Year, startDate.Date.Month, startDate.Date.Day);

                    var selMonthEnd = selMonthStart.AddMonths(1);
                    var selectedMonthRaw = intervalReadingData
                    .Where(x => x.ReadingStart >= selMonthStart &&
                                x.ReadingStart < selMonthEnd)
                    .ToList();

                    var selectedMonthTotal = Math.Round(
                        selectedMonthRaw.Sum(x => x.ReadingValue) / 1000.0, 2);
                    var hourlyBase = BuildHourlyBase(intervalReadingData);
                    //
                    var firstWeekStart = GetWeekStartYearSplit(firstDate.Date);
                    //var firstWeekStart = GetShiftedWeekStart(firstDate.Date);
                    dayGroupedData = dayGroupedData
                        .Where(x => x.Key >= firstWeekStart)
                        .ToDictionary(x => x.Key, x => x.Value);

                    nightGroupedData = nightGroupedData
                        .Where(x => x.Key >= firstWeekStart)
                        .ToDictionary(x => x.Key, x => x.Value);

                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Monthly)
                    {
                        intervalWise = intervalReadingData
                            .GroupBy(x => new
                            {
                                x.ReadingStart.Year,
                                x.ReadingStart.Month
                            })
                            .OrderBy(g => g.Key.Year)
                            .ThenBy(g => g.Key.Month)
                            .Select(g =>
                            {
                                var date = new DateTime(
                                    g.Key.Year,
                                    g.Key.Month,
                                    1);

                                return new CaclucationInterval
                                {
                                    Days = date.ToString("MMM yy"),
                                    ActualConsumption = Math.Round(
                                        g.Sum(x => x.ReadingValue) / 1000.0,
                                        2)
                                };
                            })
                            .ToList();
                    }

                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Yearly)
                    {
                        // find first valid date present in data for each year
                        var yearStartMap = intervalReadingData
                            .GroupBy(x => x.ReadingStart.Year)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Min(x => x.ReadingStart.Date) // first actual date in data
                            );

                        intervalWise = intervalReadingData
                            .Where(x =>
                            {
                                var year = x.ReadingStart.Year;

                                if (!yearStartMap.ContainsKey(year))
                                    return true;

                                // only include from first actual date of that year
                                return x.ReadingStart.Date >= yearStartMap[year];
                            })
                            .GroupBy(x => x.ReadingStart.Year)
                            .OrderBy(g => g.Key)
                            .Select(g => new CaclucationInterval
                            {
                                Days = g.Key.ToString(),
                                ActualConsumption = Math.Round(
                                    g.Sum(x => x.ReadingValue) / 1000.0, 2)
                            })
                            .ToList();
                    }
                    //before13022026
                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Weekly)
                    {
                        var groupedData = intervalReadingData
                            .GroupBy(x => GetWeekStartYearSplit(x.ReadingStart.Date))
                            .ToDictionary(
                                g => g.Key,
                                g => Math.Round(g.Sum(x => x.ReadingValue) / 1000.0, 2)
                            );

                        intervalWise = new List<CaclucationInterval>();

                        firstWeekStart = GetWeekStartYearSplit(startDate.Date);

                        for (var weekStart = firstWeekStart;
                             weekStart <= endDate.Date;
                             weekStart = weekStart.AddDays(7))
                        {
                            intervalWise.Add(new CaclucationInterval
                            {
                                Days = weekStart.ToString("yyyy-MM-dd"),
                                ActualConsumption =
                                    groupedData.ContainsKey(weekStart)
                                    ? groupedData[weekStart]
                                    : 0
                            });
                        }

                        XAxisArray = intervalWise
                            .Select(x => x.Days)
                            .ToList();
                    }

                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Daily)
                    {
                        var grouped = intervalReadingData
                            .GroupBy(x => x.ReadingStart.Date)
                            .ToDictionary(
                                g => g.Key,
                                g => Math.Round(g.Sum(x => x.ReadingValue) / 1000.0, 2)
                            );

                        intervalWise = new List<CaclucationInterval>();

                        for (var date = startDate.Date;
                             date <= endDate.Date;
                             date = date.AddDays(1))
                        {
                            intervalWise.Add(new CaclucationInterval
                            {
                                Days = date.ToString("dd MMM"),
                                ActualConsumption =
                                    grouped.ContainsKey(date)
                                    ? grouped[date]
                                    : 0
                            });
                        }

                        XAxisArray = intervalWise
                            .Select(x => x.Days)
                            .ToList();
                    }

                    var xAxisArrayValue = intervalWise.Select(t => t.Days.ToString()).ToList();
                    consumptions = new List<double>();
                    foreach (var item in XAxisArray)
                    {
                        if (xAxisArrayValue.Contains(item))
                        {
                            consumptions.Add(intervalWise.FirstOrDefault(t => t.Days.Equals(item)).ActualConsumption);
                        }
                        else
                            consumptions.Add(0);
                    }

                    double actualValue = 0;
                    var firstRecord = intervalWise.FirstOrDefault();
                    if (firstRecord != null)
                    {
                        actualValue = firstRecord.ActualConsumption;
                    }



                    List<ConsumptionDataDTO> allData = await GetMonthlyConsumptionForYear(request.ConsumptionCylceTypeId, intervalReadingData).ConfigureAwait(false);

                    List<ConsumptionDataDTO> monthlyData = await GetMonthlyConsumptionForYear(request.ConsumptionCylceTypeId, intervalReadingData).ConfigureAwait(false);
                    List<ConsumptionDataDTO> weeklyData = await GetWeeklyDataYearWise(intervalReadingData, startDate, endDate).ConfigureAwait(false);

                    List<ConsumptionDataDTO> dailyData = await GetDayData(intervalReadingData, startDate, endDate, request.ConsumptionCylceTypeId).ConfigureAwait(false);
                    //  List<ConsumptionDataDTO> 
                    weeklyData = await GetWeeklyDataYearWise(intervalReadingData, startDate, endDate).ConfigureAwait(false);
                    List<ConsumptionDataDTO> result = await GetHourly(request.ConsumptionCylceTypeId, intervalReadingData).ConfigureAwait(false);
                    // or endDate.Date
                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Weekly)
                    {
                        firstDate = intervalReadingData.Min(x => x.ReadingStart.Date);
                        var dayReadingTimeStampData =
                           intervalReadingData
                           .Select(a => new
                           {
                               Original = a,
                               SATime = a.ReadingStart   // ✅ FIX HERE
                           })
                       .Where(x =>
                           x.SATime.TimeOfDay >= dayTimeStart &&
                           x.SATime.TimeOfDay < dayTimeEnd)
                       .GroupBy(x => x.SATime.Date)
                       .Select(g => new
                       {
                           ReadingDate = g.Key,
                           ActualConsumption =
                               Math.Round(
                                   g.Sum(x => x.Original.ReadingValue) / 1000,
                                   2)
                       })
                       .ToList();


                        var nightTimeStampData = intervalReadingData
                             .Select(a => new
                             {
                                 Original = a,
                                 SATime = a.ReadingStart   // ✅ FIX HERE
                             })
                             .Where(r =>
                              (
                              r.SATime.TimeOfDay >= nightTimeStart &&
                              r.SATime.TimeOfDay <= nightTimeEnd
                             )
                              ||
                              (
                              r.SATime.TimeOfDay >= nightTimeStart2 &&
                              r.SATime.TimeOfDay < nightTimeEnd2
                              )
                              )
                              .GroupBy(x => x.SATime.Date)
                       .Select(g => new
                       {
                           ReadingDate = g.Key,
                           ActualConsumption =
                               Math.Round(
                                   g.Sum(x => x.Original.ReadingValue) / 1000,
                                   2)
                       }).ToList();

                        totalDays = (lastDate - firstDate).Days + 1;
                        var totalDayConsumption = dayReadingTimeStampData
                                                .Sum(x => x.ActualConsumption);
                        var totalNightConsumption = nightTimeStampData
                                               .Sum(x => x.ActualConsumption);
                        avgPerDay = totalDays > 0 ? totalDayConsumption / totalDays : 0;



                        avgPerNight = totalDays > 0 ? totalNightConsumption / totalDays : 0;

                    }

                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Daily)
                    {
                        firstDate = intervalReadingData.Min(x => x.ReadingStart.Date);
                        var dayReadingTimeStampData =
                           intervalReadingData
                           .Select(a => new
                           {
                               Original = a,
                               SATime = a.ReadingStart   // ✅ FIX HERE
                           })
                       .Where(x =>
                           x.SATime.TimeOfDay >= dayTimeStart &&
                           x.SATime.TimeOfDay < dayTimeEnd)
                       .GroupBy(x => x.SATime.Date)
                       .Select(g => new
                       {
                           ReadingDate = g.Key,
                           ActualConsumption =
                               Math.Round(
                                   g.Sum(x => x.Original.ReadingValue) / 1000,
                                   2)
                       })
                       .ToList();


                        var nightTimeStampData = intervalReadingData
                             .Select(a => new
                             {
                                 Original = a,
                                 SATime = a.ReadingStart   // ✅ FIX HERE
                             })
                             .Where(r =>
                              (
                              r.SATime.TimeOfDay >= nightTimeStart &&
                              r.SATime.TimeOfDay <= nightTimeEnd
                             )
                              ||
                              (
                              r.SATime.TimeOfDay >= nightTimeStart2 &&
                              r.SATime.TimeOfDay < nightTimeEnd2
                              )
                              )
                              .GroupBy(x => x.SATime.Date)
                       .Select(g => new
                       {
                           ReadingDate = g.Key,
                           ActualConsumption =
                               Math.Round(
                                   g.Sum(x => x.Original.ReadingValue) / 1000,
                                   2)
                       }).ToList();

                        totalDays = (lastDate - firstDate).Days + 1;
                        var totalDayConsumption = dayReadingTimeStampData
                                                .Sum(x => x.ActualConsumption);
                        var totalNightConsumption = nightTimeStampData
                                               .Sum(x => x.ActualConsumption);
                        avgPerDay = totalDays > 0 ? totalDayConsumption / totalDays : 0;
                        avgPerNight = totalDays > 0 ? totalNightConsumption / totalDays : 0;

                    }

                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Yearly)
                    {
                        var orderedYears = weeklyData
                            .Select(x =>
                            {
                                if (DateTime.TryParse(x.ParentID, out var d))
                                    return d.Year;

                                if (int.TryParse(x.ParentID, out var y))
                                    return y;

                                return 0;
                            })
                            .Distinct()
                            .OrderBy(x => x)
                            .ToList();

                        var lastYears = orderedYears
                            .Skip(Math.Max(0, orderedYears.Count - 2))
                            .ToList();

                        var firstYearOfRange = lastYears.First();

                        firstDate = weeklyData
                            .Where(x => GetYear(x.ParentID) == firstYearOfRange)
                            .Select(x => DateTime.Parse(x.Arg))
                            .Min();
                        int yearThreeYearsAgo = today.AddYears(-2).Year;
                        DateTime targetDate = new DateTime(yearThreeYearsAgo, 1, 1);
                        totalDays = (lastDate - targetDate).Days + 1;
                        avgPerDay = totalDays > 0 ? dtotal / totalDays : 0;
                        avgPerNight = totalDays > 0 ? ntotal / totalDays : 0;


                    }

                    if (request.ConsumptionCylceTypeId == (int)ConsumptionCylceEnum.Monthly)
                    {
                        firstDate = intervalReadingData.Min(x => x.ReadingStart.Date);
                        var orderedMonths = weeklyData
                            .Select(x => x.ParentID)
                            .Distinct()
                            .OrderBy(x => DateTime.ParseExact(
                                x,
                                "MMM yy",
                                CultureInfo.InvariantCulture))
                            .ToList();

                        var last5Months = orderedMonths
                            .Skip(Math.Max(0, orderedMonths.Count - 5))
                            .ToList();

                        var firstMonthOfRange = last5Months.First();

                        firstDate = weeklyData
                            .Where(x => x.ParentID == firstMonthOfRange)
                            .Select(x => DateTime.Parse(x.Arg))
                            .Min();

                        totalDays = (lastDate - firstDate).Days + 1;
                        avgPerDay = totalDays > 0 ? dtotal / totalDays : 0;
                        avgPerNight = totalDays > 0 ? ntotal / totalDays : 0;
                        // ✅ correct week start without going to previous month
                        var tempWeekStart = GetWeekStartYearSplit(firstDate.Date);

                        if (tempWeekStart.Month != firstDate.Month)
                            firstWeekStart = firstDate.Date;
                        else
                            firstWeekStart = tempWeekStart;


                        for (var weekStart = firstWeekStart;
                             weekStart <= endDate.Date;
                             weekStart = weekStart.AddDays(7))
                        {
                            dayIntervalWise.Add(new CaclucationInterval
                            {
                                Days = weekStart.ToString("yyyy-MM-dd"),
                                ActualConsumption =
                                    dayGroupedData.ContainsKey(weekStart)
                                    ? dayGroupedData[weekStart]
                                    : 0
                            });

                            nightIntervalWise.Add(new CaclucationInterval
                            {
                                Days = weekStart.ToString("yyyy-MM-dd"),
                                ActualConsumption =
                                    nightGroupedData.ContainsKey(weekStart)
                                    ? nightGroupedData[weekStart]
                                    : 0
                            });
                        }
                    }

                    var data = new ConsumptionDashboardDto()
                    {
                        AerageConsumption = new AverageConsumption
                        {
                            //Daily = double.Round(intervalWise.Sum(t => t.ActualConsumption) / XAxisArray.Count(), 2, MidpointRounding.AwayFromZero),
                            //DayTime = double.Round(dayReadingTimeStampData.Sum(t => t.ActualConsumption) / XAxisArray.Count(), 2, MidpointRounding.AwayFromZero),
                            //NightTime = double.Round(nightReadingTimeStampData.Sum(t => t.ActualConsumption) / XAxisArray.Count(), 2, MidpointRounding.AwayFromZero),

                            Daily = double.Round(consumptions.Sum() / totalDays, 2, MidpointRounding.AwayFromZero),
                            //DayTime = double.Round(dayReadingTimeStampData.Sum(t => t.ActualConsumption) / totalDays, 2, MidpointRounding.AwayFromZero),
                            DayTime = double.Round(avgPerDay, 2, MidpointRounding.AwayFromZero),
                            NightTime = double.Round(avgPerNight, 2, MidpointRounding.AwayFromZero),
                        },
                        ConsumptionCycleId = request.ConsumptionCylceTypeId,
                        ConsumptionCylceType = enumConsumptionCylce.ToString(),
                        GuageChartDto = new GuageChartDto()
                        {
                            ActualValue = actualValue,
                            TargetValue = guageDailyConsumption
                        },
                        MeterUnit = targetValueByMeter.UnitOfMeasure,
                        LineChartDto = new LineChartDto
                        {
                            XAxisdata = XAxisArray,
                            SeriesLineData = consumptions
                        }

                    };
                    data.MonthlyData = monthlyData;
                    data.WeeklyData = weeklyData;
                    data.DailyData = dailyData;
                    data.HourlyData = result;

                    return data;
                }

            }
            consumptions = new List<double>()
                        {
                           0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
                        };

            blankData = new ConsumptionDashboardDto()
            {
                AerageConsumption = new AverageConsumption
                {
                    Daily = 0,
                    DayTime = 0,
                    NightTime = 0,
                },
                ConsumptionCycleId = request.ConsumptionCylceTypeId,
                ConsumptionCylceType = enumConsumptionCylce.ToString(),
                GuageChartDto = new GuageChartDto()
                {
                    ActualValue = 0,
                    TargetValue = guageDailyConsumption
                },
                MeterUnit = targetValueByMeter.UnitOfMeasure,
                LineChartDto = new LineChartDto
                {
                    XAxisdata = XAxisArray,
                    SeriesLineData = consumptions
                }
            };
            return blankData;

        }
        private int GetYear(string value)
        {
            if (int.TryParse(value, out var y))
                return y;

            if (DateTime.TryParse(value, out var d))
                return d.Year;

            if (DateTime.TryParseExact(
                value,
                "MMM yy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var m))
                return m.Year;

            return 0;
        }

        // before 13042026
        private async Task<List<ConsumptionDataDTO>> GetWeeklyDataYearWise(List<Reading> intervalReadingData, DateTime startDate, DateTime endDate)
        {
            var result = new List<ConsumptionDataDTO>();

            var currentMonth = new DateTime(startDate.Year, startDate.Month, 1);

            while (currentMonth <= endDate)
            {
                var monthStart = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var weekStart = monthStart;
                int weekCounter = 1;

                while (weekStart <= monthEnd)
                {
                    var weekEnd = weekStart.AddDays(6 - (int)weekStart.DayOfWeek);

                    if (weekEnd > monthEnd)
                        weekEnd = monthEnd;

                    //✅ direct calculation(NO grouping mismatch)
                    var total = intervalReadingData
                        .Where(x => x.ReadingStart.Date >= weekStart &&
                                    x.ReadingStart.Date <= weekEnd)
                        .Sum(x => x.ReadingValue);

                    result.Add(new ConsumptionDataDTO
                    {
                        Arg = weekStart.ToString("yyyy-MM-dd"),
                        UniqueId = $"{currentMonth:yyyy-MM}-W{weekCounter}",
                        Val = Math.Round(total / 1000.0, 2),
                        ParentID = currentMonth.ToString("MMM yy")
                    });

                    weekStart = weekEnd.AddDays(1);
                    weekCounter++;
                }

                currentMonth = currentMonth.AddMonths(1);
            }

            return result.OrderBy(x => x.Arg).ToList();
        }

        private List<HourlyBucket> BuildHourlyBase(List<Reading> intervalReadingData)
        {
            return intervalReadingData
                .Select(r => new
                {
                    Day = r.ReadingStart.Date,              // 🔥 consistent
                    Hour = r.ReadingStart.Hour,             // 🔥 consistent
                    Value = r.ReadingValue / 1000.0
                })
                .GroupBy(x => new { x.Day, x.Hour })
                .Select(g => new HourlyBucket
                {
                    Day = g.Key.Day,
                    Hour = g.Key.Hour,
                    HourlyConsumption = Math.Round(g.Sum(x => x.Value), 2)
                })
                .OrderBy(x => x.Day)
                .ThenBy(x => x.Hour)
                .ToList();
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            // Sunday-based week (match your UI)
            int diff = date.DayOfWeek - DayOfWeek.Sunday;
            if (diff < 0) diff += 7;
            return date.AddDays(-diff).Date;
        }
        private async Task<List<ConsumptionDataDTO>> GetDayData(List<Reading> readings, DateTime startDate, DateTime endDate, int cycleId)
        {
            var grouped = readings
                .GroupBy(x => x.ReadingStart.Date)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Sum(x => x.ReadingValue) / 1000.0, 2)
                );

            var totalDays = (endDate.Date - startDate.Date).Days + 1;

            var result = Enumerable.Range(0, totalDays)
                .Select(offset => startDate.Date.AddDays(offset))
                .Select(date =>
                {
                    //var weekId = GetMonthWeekId(date); // ✅ match weekly uniqueId

                    return new ConsumptionDataDTO
                    {
                        Arg = date.ToString("dd MMM"),
                        UniqueId = date.ToString("yyyy-MM-dd"),

                        Val = grouped.ContainsKey(date)
                                ? grouped[date]
                                : 0,

                        // ✅ SAME AS WEEK UNIQUEID
                        //ParentID = GetMonthWeekId(date)
                        ParentID = cycleId == (int)ConsumptionCylceEnum.Weekly
                    ? GetIsoWeekStart(date).ToString("yyyy-MM-dd")   // ✅ weekly
                    : GetMonthWeekId(date)
                    };
                })
                .ToList();

            return result;
        }

        private async Task<List<ConsumptionDataDTO>> GetHourly(int consumptionCycleTypeId, List<Reading> intervalReadingData)
        {
            var data = new List<ConsumptionDataDTO>();

            var hourlyBuckets = intervalReadingData
                .Select(r => new { Day = r.ReadingStart.Date, Hour = r.ReadingStart.Hour, Value = r.ReadingValue / 1000.0 })
                .GroupBy(x => new { x.Day, x.Hour })
                .Select(g => new { g.Key.Day, g.Key.Hour, HourlyConsumption = Math.Round(g.Sum(x => x.Value), 2) })
                .ToList();

            foreach (var dayGroup in hourlyBuckets.GroupBy(x => x.Day).OrderBy(x => x.Key))
            {
                // ✅ UNIQUE ID for EVERY day (no conflicts across years/weeks)
                string dayUniqueId = dayGroup.Key.ToString("yyyy-MM-dd");  // "2026-02-04"
                string dayDisplay = dayGroup.Key.ToString("dd MMM");       // "04 Feb" 

                for (int h = 0; h < 24; h++)
                {
                    var hour = dayGroup.FirstOrDefault(x => x.Hour == h);
                    data.Add(new ConsumptionDataDTO
                    {
                        Arg = $"{h:00}:00",
                        Val = hour?.HourlyConsumption ?? 0,
                        ParentID = dayUniqueId  // "2026-02-04" ← UNIQUE, no conflicts
                    });
                }
            }
            return data;
        }

        private DateTime GetWeekStartFixed(DateTime date)
        {
            var weekStart = GetWeekStart(date);

            // if weekStart is in previous year but date is new year
            if (weekStart.Year < date.Year)
            {
                return new DateTime(date.Year, 1, 1);
            }

            return weekStart;
        }


        private DateTime GetWeekStartYearSplit(DateTime date)
        {
            //var weekStart = GetWeekStart(date);
            var weekStart = GetWeekStartFixed(date);


            // if week belongs to previous year but date is new year
            if (weekStart.Year < date.Year)
            {
                // start from Jan 1
                return new DateTime(date.Year, 1, 1);
            }

            return weekStart;
        }
        private async Task<List<ConsumptionDataDTO>> GetMonthlyConsumptionForYear(int consumptionCycleId, List<Reading> intervalReadingData)
        {
            var data = new List<ConsumptionDataDTO>();
            var monthGroups = intervalReadingData
                            .GroupBy(x => new
                            {
                                x.ReadingStart.Year,
                                x.ReadingStart.Month
                            })
                            .ToDictionary(
                                g => new DateTime(g.Key.Year, g.Key.Month, 1),
                                g => g.Sum(x => x.ReadingValue)
                            );


            var minYear = intervalReadingData.Min(x => x.ReadingStart.Year);
            var maxYear = intervalReadingData.Max(x => x.ReadingStart.Year);


            for (int year = minYear; year <= maxYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    var date = new DateTime(year, month, 1);

                    var key = date;

                    var total = monthGroups.ContainsKey(key)
                        ? monthGroups[key]
                        : 0;

                    data.Add(new ConsumptionDataDTO
                    {
                        Arg = date.ToString("MMM yy"),
                        Val = Math.Round(total / 1000.0, 2),
                        ParentID = year.ToString()
                    });
                }
            }

            return data;
        }

        public (int Year, int Week) GetIsoWeek(DateTime date)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;

            int week = cal.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            int year = date.Year;

            // ISO year correction
            if (week >= 52 && date.Month == 1)
                year--;

            if (week == 1 && date.Month == 12)
                year++;

            return (year, week);
        }
        public DateTime GetIsoWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return date.AddDays(-diff).Date;
        }

    }






}

