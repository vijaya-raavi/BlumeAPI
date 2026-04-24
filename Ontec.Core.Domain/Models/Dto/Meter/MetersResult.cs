namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class MetersUtilityDto
    {
        public string MeterNumbers { get; set; }
        public int MeterTypeId { get; set; }
        public double MinDailyTarget { get; set; }
        public double MaxDailyTarget { get; set; }
        public string MeterUtility { get; set; }
        public string UnitOfMeasure { get; set; }
    }
}
