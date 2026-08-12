namespace Ontec.Core.Domain.Models.Dto.Property
{
    public class PropertyActiveDto
    {
        public IEnumerable<PropertyMeter> MeterList { get; set; }
        public string Message { get; set; }
    }
    public class PropertyMeter
    {

        public int MeterId { get; set; }
        public string MeterNumber { get; set; }
        public int MeterStatusId { get; set; }
        public string MeterStatus { get; set; }
        public string PropertyUnitNumber { get; set; }
        public string LinkedProperty { get; set; }
    }
    public class LinkedProperty
    {
        public string MeterNumber { get; set; }
        public int LinkedPropertyId { get; set; }
        public string LinkedPropertyName { get; set; }
        public string LinkedPropertyUnitNumber { get; set; }
        public string LinkedPropertyMeterStatus { get; set; }
    }
}
