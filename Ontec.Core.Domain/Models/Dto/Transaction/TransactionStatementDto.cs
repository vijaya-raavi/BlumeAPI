namespace Ontec.Core.Domain.Models.Dto.Transaction
{

    public class TCustomerTransItem
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal VAT { get; set; }
        public decimal Units { get; set; }
        public string Tariff { get; set; }

        public string TransactionItemType { get; set; }
    }

    public class TransactionData
    {
        public TransactionData()
        {
            Details = [];
        }
       
        public string Meter { get; set; }
        public decimal Unit { get; set; }
        public string Date { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TenderedAmount {  get; set; }
        public decimal? ResultantBalance {  get; set; }
        public string AccountTransType {  get; set; }
        public string? ReceiptNumber {  get; set; }
        public string? OurRef {  get; set; }
        public double TransactionFee { get; set; }
        public string TransactionId { get; set; }
        public string Tariff {  get; set; }
        public string Comment {  get; set; }
        public List<TCustomerTransItem> Details { get; set; } 


    }

    public class TransactionStatementDto
    {
        public PropertyUserDetail PropertyUserDetails { get; set; }
        public IEnumerable<AccountAdjustment> AccountAdjustments { get; set; }
        public Total AdjustmentTotal { get; set; }

        public IEnumerable<TransactionData> TransactionStatements { get; set; }
    }

    public class TransactionStatement
    {
        public string MeterType { get; set; }

        public string MeterNumber { get; set; }

        public string Unit { get; set; }

        public IEnumerable<TransactionModel> Transactions { get; set; } = new List<TransactionModel>();

        public Total Totals { get; set; }
    }

    public class Total
    {
        public decimal Usage { get; set; }
        public decimal Cost { get; set; }
        public decimal Vat { get; set; }
        public decimal Vending { get; set; }
        public decimal Network { get; set; }
        public decimal TotalR { get; set; }
    }
    public class TransactionModel : Total
    {
        public string TransactionDate { get; set; }
    }
    public class AccountAdjustment
    {
        public string Date { get; set; }
        public string Description { get; set; }
        public decimal Total { get; set; }
    }
    public class PropertyUserDetail
    {
        public string Owner { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Complex { get; set; }
        public string CompanyLogo { get; set; }

        public string Account {  get; set; }

        public string TaxNumber { get; set; }
    }
}