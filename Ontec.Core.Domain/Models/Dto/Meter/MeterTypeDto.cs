namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class MeterTypeDto
    {
        public IEnumerable<MeterTypes>? MeterType { get; set; }

    }

    public class MeterTypes
    {
        public int id { get; set; }
        public string name { get; set; }
        public string UnitOfMeasure { get; set; }
        public string  MinDailyTarget { get; set; }
        public string MaxDailyTarget { get; set; }
    }
}
