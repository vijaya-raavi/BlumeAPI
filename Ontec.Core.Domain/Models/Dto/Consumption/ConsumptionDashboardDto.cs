using Ontec.Core.Domain.Models.Dto.Charts;

namespace Ontec.Core.Domain.Models.Dto.Consumption
{
    public class ConsumptionDashboardDto
    {
        public string ConsumptionCylceType { get; set; }
        public int ConsumptionCycleId { get; set; }
        public string MeterUnit { get; set; }
        public AverageConsumption AerageConsumption { get; set; }
        public GuageChartDto GuageChartDto { get; set; }
        public LineChartDto LineChartDto { get; set; }
        public List<ConsumptionDataDTO> HourlyData { get; set; }
        public List<ConsumptionDataDTO> MonthlyData { get; set; }
        public List<ConsumptionDataDTO> WeeklyData { get; set; }
        public List<ConsumptionDataDTO> DailyData { get; set; }

        public List<ConsumptionDataDTO> HierarchicalData { get; set; }
    }
    public class WeeklyAverageConsumption
    {
        public double Daily { get; set; }
        public double DayTime { get; set; }
        public double NightTime { get; set; }
    }
    public class WeeklyConsumptionDashboardDto
    {
        public string ConsumptionCylceType { get; set; }
        public int ConsumptionCycleId { get; set; }
        public string MeterUnit { get; set; }
        public WeeklyAverageConsumption AerageConsumption { get; set; }
        public WeeklyGuageChartDto GuageChartDto { get; set; }
        public WeeklyLineChartDto LineChartDto { get; set; }



    }
    public class DailyConsumptionDashboardDto
    {
        public string ConsumptionCylceType { get; set; }
        public int ConsumptionCycleId { get; set; }
        public string MeterUnit { get; set; }
        public DailyAverageConsumption AerageConsumption { get; set; }
        public DailyGuageChartDto GuageChartDto { get; set; }
        public DailyLineChartDto LineChartDto { get; set; }



    }
    public class DailyAverageConsumption
    {
        public double Daily { get; set; }
        public double DayTime { get; set; }
        public double NightTime { get; set; }
    }
    public class AverageConsumption
    {
        public double Daily { get; set; }
        public double DayTime { get; set; }
        public double NightTime { get; set; }
    }

    public class ConsumptionDataDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UniqueId { get; set; }
        public int Year {  get; set; }
        public string Month {  get; set; }
        public string Arg { get; set; }
        public double Val { get; set; }
        public string ParentID { get; set; }
    }
    public class DayGroup
    {
        public string Day { get; set; }
        public double DailyConsumption { get; set; }
    }
    public class MonthGroup
    {
        public int Year { get; set; }
        public string Month { get; set; }
        public int M {  get; set; }
        public double DailyConsumption { get; set; }
    }

    public class WeekGroup
    {
        public string Week { get; set; }
        public double TotalConsumption { get; set; }
        public List<DayGroup> Days { get; set; }
    }

    public class YearGroup
    {
        public string Year { get; set; }
        public double TotalConsumption { get; set; }
        public List<MonthGroup> Months { get; set; }
    }

    public class CalculationInterval
    {
        public string Days { get; set; }
        public double ActualConsumption { get; set; }
    }
    public class ProjectedReading
    {
        public Reading Original { get; set; }
        public DateTimeOffset UtcDateTimeOffset { get; set; } // absolute instant in UTC
        public DateTime SaDateTime { get; set; } // SA local time as DateTime (Kind=Unspecified/Local)
        public DateTime SaDate { get; set; } // date only
        public double ReadingValue { get; set; }
    }
    public class WeekGroup1
    {
        public int WeekNumber { get; set; }
        public double TotalConsumption { get; set; }
        public List<DayGroup1> Days { get; set; } = new();
        public string WeekLabel => $"W{WeekNumber}";
    }

    public class DayGroup1
    {
        public DateTime Date { get; set; }
        public string Label => Date.ToString("dd MMM");
        public double DailyConsumption { get; set; }
    }
    public class MonthGroup1 { public int Year { get; set; } public int Month { get; set; } public string Label => new DateTime(Year, Month, 1).ToString("MMM yy"); public double TotalConsumption { get; set; } }
    public class YearGroup1 { public int Year { get; set; } public double TotalConsumption { get; set; } }
    public class CaclucationInterval
    {
        public DateTime Date { get; set; }   // new
        public DateTime StartDate {  get; set; }
        public DateTime EndDate {  get; set; }
        public string Days { get; set; }
        public double ActualConsumption { get; set; }
        
       
    }
    public class HourlyBucket
    {
        public DateTime Day { get; set; }
        public int Hour { get; set; }
        public double HourlyConsumption { get; set; }
    }
    public class HierarchyDto
    {
        public string Arg { get; set; }        // X-axis label (Year / Month / Day / Hour)
        public double Val { get; set; }         // Consumption value
        public string ParentID { get; set; }    // Parent for drill-down
    }
   
   
}