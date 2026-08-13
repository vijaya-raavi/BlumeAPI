using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Models.Dto.Dashboard
{
    public class DashboardMeterDayConsumptionDto
    {
        public int MeterId { get; set; }
        public string MeterType { get; set; }
        public string MasterMeterType { get; set; }
        public string MeterNumber { get; set; }
        public string DailyTargetConsumption { get; set; }
        public string Consumption { get; set; }
        public string UnitOfMeasure { get; set; }
        public string MeterReadingType { get; set; }
        public Boolean IsSolar {  get; set; }
        public GuageChartDto GuageChartDto { get; set; }
        public SolarConsumptionDTO SolarData { get; set; }

        public GetSTSTopUpTransactions STSTopUpTransactions { get; set; }
    }

    public class SolarConsumptionDTO
    {
        public string MeterNumber { get; set; }
        public double InstalledCapacity { get; set; }
        public double ExportedToday { get; set; }
        public double TodayPercentage { get; set; }
        public double ExportedForMonth { get; set; }

    }
}
