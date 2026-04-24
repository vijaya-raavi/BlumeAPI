using Ontec.Core.Domain.Models.Dto.Company;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Models.Dto.Transaction
{

    public class TransactionSummaryDto
    {
        public IEnumerable<TransactionData> Transaction { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalBillingcalculations { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalAdjustments { get; set; }
        public decimal ClosingBalance { get; set; }
    }
    public class TransactionPdfViewModel
    {
        public string LogoUrl { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyMobile { get; set; }
        public string CountryName { get; set; }
        public string VAT { get; set; }
        public string TransactionPeriod { get; set; }
        public string CustomerName { get; set; }
        public string CustomerComplex { get; set; }

        public string Address { get; set; }
        public string GeneratedDate { get; set; }

        public string AccountNo { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string CustomerVatNo { get; set; }


        public Total AdjustmentTotal { get; set; }

        public decimal OpeningBalance { get; set; }
        public decimal TotalBillingcalculations { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalAdjustments { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal ElectricVat { get; set; }
        public decimal ElectricIncludeVat { get; set; }
        public decimal WaterVat { get; set; }
        public decimal WaterIncludeVat { get; set; }
        public decimal GasVat { get; set; }
        public decimal GasIncludeVat { get; set; }
        public bool showPayments { get; set; }
        public bool ShowAdjustments { get; set; }
        public bool ShowGroupedTransactions { get; set; }
        public string TotalDepTax { get; set; }
        public string TotalDepAmount { get; set; }

        public string TotalAdjustmentTax { get; set; }
        public string TotalAdjustmentamount { get; set; }
        public List<Deposit> Payments { get; set; }
        public List<Adjustment> Adjustments { get; set; }
        public string Key { get; set; }
        public List<GroupTransactions> MeterTransactions { get; set; }

        public string TotalAmount {  get; set; }
        public string TotalUnit {  get; set; }
    }
    public class Deposit
    {
        public string Date { get; set; }
        public string ReceiptNumber { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

    }
    public class Adjustment
    {
        public string Date { get; set; }
        public string ReceiptNumber { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

    }
    public class GroupTransactions
    {
        public string Date { get; set; }
        public string Tariff { get; set; }
        public double Usage { get; set; }
        public double VAT { get; set; }
        public double Amount { get; set; }
    }
}
