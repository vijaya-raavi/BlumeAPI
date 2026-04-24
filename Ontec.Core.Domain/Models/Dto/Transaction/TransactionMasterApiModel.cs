namespace Ontec.Core.Domain.Models.Dto.Transaction
{
    public class TransactionMasterApiModel
    {
        public List<Transaction> Data { get; set; }
    }
    public class CustomerTransItem
    {
        public string Id { get; set; }
        public string TransItemType { get; set; }
        public decimal AmtInclTax { get; set; }
        public decimal AmtTax { get; set; }
        public string Token { get; set; }
        public string Description { get; set; }
        public string ReceiptNum { get; set; }
        public decimal OpenBalance { get; set; }
        public decimal RemBalance { get; set; }
        public decimal Units { get; set; }
        public string TariffId { get; set; }
        public string Tariff { get; set; }
        public string Decimal { get; set; }
    }
    public class Transaction
    {
        public string Id { get; set; }
        public List<CustomerTransItem> CustomerTransItems { get; set; }
        public string CustomerAgreementId { get; set; }
        public string UsagePointId { get; set; }
        public string MeterId { get; set; }
        public string Client { get; set; }
        public string Terminal { get; set; }
        public string VendRefReceived { get; set; }
        public string ReceiptNum { get; set; }
        public string MeterNumber { get; set; }
        public DateTime TransDate { get; set; }
        public decimal AmtTax { get; set; }
        public decimal AmtInclTax { get; set; }
        public bool HasEngineeringTokens { get; set; }
        public string PayType { get; set; }
        public string CustomerAccountId { get; set; }
        public string CustomerTransType { get; set; }
        public string PaymentMode { get; set; }
        public string ServiceResource { get; set; }
        public string MeterType { get; set; }
        public bool Reversed { get; set; }
        public string PricingStructureId { get; set; }
        public string TariffId { get; set; }
        public string Tariff { get; set; }
        public DateTime LastReprintDt { get; set; }
    }
}
