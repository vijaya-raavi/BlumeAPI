namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class MeterConsumptionUnitDto
    {
        public double DailyTargetConsumption { get; set; }
        public string UnitOfMeasure { get; set; }
        public string MeterReadingType { get; set; }
    }
}
