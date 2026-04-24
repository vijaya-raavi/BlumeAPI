namespace Ontec.Core.Domain.Models.Dto.Consumption
{
    //public class CaclucationInterval
    //{
    //    public string Days { get; set; }
    //    public double ActualConsumption { get; set; }
    //}
    public class IntervalMeterReadingDataModel
    {
        public List<Reading> Data { get; set; }
    }
    public class Reading
    {
        public string Id { get; set; }
        public string MeterId { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset ReadingTimestamp { get; set; }
        public DateTimeOffset ReadingStart { get; set; }
        public DateTimeOffset ReadingEnd { get; set; }
        public double ReadingValue { get; set; }
        public string MeterReadingType { get; set; }
        public string UsagePointId { get; set; }
        public bool Estimate { get; set; }
        public bool Test { get; set; }
        public bool Manual { get; set; }
    } 
}
