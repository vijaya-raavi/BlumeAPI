namespace Ontec.Core.Domain.Models.Dto.TopUp
{


    public class GetSTSTopUpTransactions
    {
        public decimal TotalPurchase { get; set; }

        public IEnumerable<STSTopUpTransactions> STSTopUpTransaction { get; set; }
        public class STSTopUpTransactions
        {
            public string MeterNumber { get; set; }
            public string UserId { get; set; }
            public string MeterId { get; set; }
            public decimal DebtAmount { get; set; }
            public string CreatedAt { get; set; }
            public string StdToken { get; set; }
            public string BsstToken { get; set; }
            public string KeyChangeToken { get; set; }
            public decimal stdUnits { get; set; }
            public decimal stdAmt { get; set; }
            public string Tarrif { get; set; }
            public List<string> TarrifUnits { get; set; }
            public string TxnId { get; set; }
            public bool IsInHouseTransaction { get; set; }
            public string AMIMeterTarrif { get; set; }
            public string PaymentGateWay { get; set; }
            public decimal debtTax { get; set; }
            public string VendResponse { get; set; }
            public decimal TransactionFee { get; set; }


        }



    }
}
