using System.Text.Json.Serialization;

namespace Ontec.Core.Domain.Common
{
    public class MasterApiSetting
    {
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; }

        [JsonPropertyName("meterNumberApi")]
        public string MeterNumberApi { get; set; }

        [JsonPropertyName("intervalReadingApi")]
        public string IntervalReadingApi { get; set; }

        [JsonPropertyName("auxAccountApi")]
        public string AuxAccountApi { get; set; }

        [JsonPropertyName("auxChargeScheduleApi")]
        public string AuxChargeScheduleApi { get; set; }

        [JsonPropertyName("accountTransApi")]
        public string AccountTransApi { get; set; }

        [JsonPropertyName("unitsTransApi")]
        public string UnitsTransApi { get; set; }

        [JsonPropertyName("customerTransApi")]
        public string CustomerTransApi { get; set; }

        [JsonPropertyName("debitechApiKey")]
        public string DebitechApiKey { get;set; }
    }
}
