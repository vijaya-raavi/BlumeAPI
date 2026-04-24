using Newtonsoft.Json;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public class UpdateNotifyTopUpTransactionQuery
    {
            [JsonProperty("m_payment_id")]
            public string MPaymentId { get; set; }

            [JsonProperty("pf_payment_id")]
            public string PfPaymentId { get; set; }

            [JsonProperty("payment_status")]
            public string PaymentStatus { get; set; }

            [JsonProperty("item_name")]
            public string ItemName { get; set; }

            [JsonProperty("item_description")]
            public string ItemDescription { get; set; }

            [JsonProperty("merchant_id")]
            public string MerchantId { get; set; }

            [JsonProperty("signature")]
            public string Signature { get; set; }
        }

    
}
