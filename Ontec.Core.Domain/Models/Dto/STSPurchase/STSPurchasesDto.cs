using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Ontec.Core.Domain.Models.Dto.STSPurchase
{
    public class STSPurchasesDto
    {
        public class Debt
        {
            [JsonProperty("@amt")]
            public string amt { get; set; }

            [JsonProperty("@tax")]
            public string tax { get; set; }

            [JsonProperty("@rem")]
            public string rem { get; set; }

            [JsonProperty("@desc")]
            public string desc { get; set; }

            [JsonProperty("@open")]
            public string open { get; set; }

            [JsonProperty("@accountType")]
            public string accountType { get; set; }

            [JsonProperty("@rctNum")]
            public string rctNum { get; set; }

            [JsonProperty("#text")]
            public string text { get; set; }
        }

        public class ElecMsg
        {
            [JsonProperty("@ver")]
            public string ver { get; set; }

            [JsonProperty("@service")]
            public string service { get; set; }
            public ReprintManyRes reprintManyRes { get; set; }
        }

        public class IpayMsg
        {
            [JsonProperty("@client")]
            public string client { get; set; }

            [JsonProperty("@term")]
            public string term { get; set; }

            [JsonProperty("@seqNum")]
            public string seqNum { get; set; }

            [JsonProperty("@time")]
            public string time { get; set; }
            public static ElecMsg ElecMsg { get; set; }
        }

        public class ReprintManyRes
        {
            public string @ref { get; set; }
            public Res res { get; set; }

            [XmlArray("VendResponses")]
            [XmlArrayItem("VendRespinse")]
            public  List<VendRe> vendRes { get; set; }
        }

        public class Res
        {
            [JsonProperty("@code")]
            public string code { get; set; }

            [JsonProperty("#text")]
            public string text { get; set; }
        }

        public class Root
        {
            public IpayMsg ipayMsg { get; set; }
        }

        public class Util
        {
            [JsonProperty("@addr")]
            public string addr { get; set; }

            [JsonProperty("@contact")]
            public string contact { get; set; }

            [JsonProperty("@taxRef")]
            public string taxRef { get; set; }

            [JsonProperty("@distId")]
            public string distId { get; set; }

            [JsonProperty("@receiptFormat")]
            public string receiptFormat { get; set; }

            [JsonProperty("#text")]
            public string text { get; set; }
        }

        public class VendRe
        {
            [JsonProperty("@resource")]
            public string resource { get; set; }

            [JsonProperty("@isRefund")]
            public string isRefund { get; set; }

            [JsonProperty("@meterNumber")]
            public string meterNumber { get; set; }

            [JsonProperty("@transDate")]
            public string transDate { get; set; }

            [JsonProperty("@payType")]
            public string payType { get; set; }
            public string @ref { get; set; }
            public Res res { get; set; }
            public Util util { get; set; }
            public string tariff { get; set; }
            public List<Debt> debt { get; set; }
        }

    }
}
