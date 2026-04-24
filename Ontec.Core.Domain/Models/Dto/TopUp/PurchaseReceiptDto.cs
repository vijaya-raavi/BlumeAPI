using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class PurchaseReceiptDto
    {
        public decimal ActualRechargeAmount { get; set; }
        public string UtilName { get; set; }
        public string UtilDistId { get; set; }
        public string UtilVATNo { get; set; }
        public string UtilAddress { get; set; }

        public string ReferenceNmber { get; set; }
        public DateTime IssuedDate { get; set; }
        public string MeterNumber { get; set; }
        public string ReceiptId { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal TransactionFee { get; set; }
        public string StandardTokens { get; set; }
        public string VendorName { get; set; }
        public string DomesticTarrif { get; set; }
        public string LogoUrl { get; set; }
        public string TaxNumber { get; set; }
        public string Customer { get; set; }
        public string Address { get; set; }
        public string TokenTech { get; set; }
        public string Alg { get; set; }
        public string SGC { get; set; }
        public string KRN { get; set; }
        public string TI { get; set; }
        public decimal Units { get; set; }
        public string? StdTarrif { get; set; }

        public decimal? stdamt { get; set; }
        public decimal stdtax { get; set; }
        public string? stdReceiptId { get; set; }
        public decimal? RemainingBalance { get; set; }
        public decimal PurchasePriceInclTax { get; set; }
        public decimal PurchasePriceExlTax { get; set; }

        public string KeyChangeToken { get; set; }
        public string? BsstToken { get; set; }

        public decimal? BsstTokenUnits { get; set; }
        public decimal? BsstTokenAmount { get; set; }
        public decimal? BsstTokenTax { get; set; }
        public string BsstReceiptId { get; set; }

        public string OldSGC { get; set; }
        public string OldTI { get; set; }
        public string OldKRN { get; set; }

        public string NewKRN { get; set; }
        public string NewTI { get; set; }
        public string NewSGC { get; set; }

        public string CustomerMessage { get; set; }
        public string VendResponse { get; set; }
        public string RctNo { get; set; }
        public string RCTNo { get; set; }
        public bool IsInHouseTxn { get; set; }
    }
    public class ReceiptViewModel
    {
        public decimal ActualRechargeAmount { get; set; }
        public string UtilName { get; set; }
        public string UtilDistId { get; set; }
        public string UtilVATNo { get; set; }
        public string UtilAddress { get; set; }
        public string ReferenceNmber { get; set; }
        public DateTime IssuedDate { get; set; }
        public string MeterNumber { get; set; }
        public string ReceiptId { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal TransactionFee { get; set; }
        public string StandardTokens { get; set; }
        public string VendorName { get; set; }
        public string DomesticTarrif { get; set; }
        public string LogoUrl { get; set; }
        public string TaxNumber { get; set; }
        public string Customer { get; set; }
        public string Address { get; set; }
        public string TokenTech { get; set; }
        public string Alg { get; set; }
        public string SGC { get; set; }
        public string KRN { get; set; }
        public string TI { get; set; }
        public decimal Units { get; set; }
        public string? StdTarrif { get; set; }

        public decimal? stdamt { get; set; }
        public decimal stdtax { get; set; }
        public string? stdReceiptId { get; set; }
        public decimal? RemainingBalance { get; set; }
        public decimal PurchasePriceInclTax { get; set; }
        public decimal PurchasePriceExlTax { get; set; }

        public string KeyChangeToken { get; set; }
        public string? BsstToken { get; set; }

        public decimal? BsstTokenUnits { get; set; }
        public decimal? BsstTokenAmount { get; set; }
        public decimal? BsstTokenTax { get; set; }
        public string BsstReceiptId { get; set; }

        public string OldSGC { get; set; }
        public string OldTI { get; set; }
        public string OldKRN { get; set; }

        public string NewKRN { get; set; }
        public string NewTI { get; set; }
        public string NewSGC { get; set; }

        public string CustomerMessage { get; set; }
        public string VendResponse { get; set; }
        public string RctNo { get; set; }
        public bool IsInHouseTxn { get; set; }
        public bool ShowKeyChange { get; set; }
        public bool ShowStdToken { get; set; }
        public bool ShowDebt { get; set; }
        public bool ShowFixed { get; set; }
        public bool ShowTokenDetails { get; set; }
        public bool ShowBsstToken { get; set; }
        public bool ShowTxnFee { get; set; }
        public bool ShowCustomerMessage { get; set; }
        public bool ShowTarrifToken {  get; set; }
        public bool ShowTaxDetails { get; set; }
        public string Tariff { get; set; }
        public List<DebtItem> DebtItems { get; set; } 
        public List<FixedItem> FixedItems { get; set; } 
        

    }
    public class DebtItem
    {
        public string Text {  get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal RemainBalance { get; set; }
    }
  
    public class FixedItem
    {
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public string Text { get; set; }
    }
}
