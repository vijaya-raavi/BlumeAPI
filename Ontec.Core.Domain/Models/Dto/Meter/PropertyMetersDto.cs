namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class PropertyMetersDto
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public bool IsOwner { get; set; }
        public int TotalMeter { get; set; }
        public int InCompleteMeterCount { get; set; }

        public int SolarMeterCount {  get; set; }
        public IEnumerable<MeterDto> Meters { get; set; }
        
    }
}
