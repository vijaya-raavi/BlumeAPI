namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class MeterRequestDto
    {
        public int MeterID { get; set; }
        public string MeterDetails {  get; set; }
        public string PropertyAddress {  get; set; }
        public string Customer {  get; set; }
        public string Document { get; set; }
        public string Date {  get; set; }
        public string UnitOfMeasure { get; set; }
        public string MeterAlias {  get; set; }
        public string MeterNumber { get; set;}
        public string UtilityType { get; set; }
        public int TargetConsumption { get; set; }
        public string UnitNumber { get; set; }
        public string PropertyTitle { get; set; }


    }
}
