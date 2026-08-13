namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public  class GetTopUpTransaction 
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public int UserId { get; set; }
        public bool UseWallet { get; set; }
        public int MeterId { get; set; }
        public int MeterTypeId { get; set; }
        public string MasterMeterType { get; set; }
        public string MeterType { get; set; }
        public string UnitOfMeasure { get; set; }
        public string MeterNumber { get; set; }
        public string TransactionID { get; set; }
        public string TransactionFee { get; set; }
        public double RechargeAmount { get; set; }
        public double DebtAmount { get; set; }
        public string CreatedAt { get; set; }
        public string EFTNumber { get; set; }
        public string ModifiedAt {  get; set; }
        public string Flag { get; set; }

        public string PayFastResponse {  get; set; }
        public string TopupStatus { get; set; }
        public string VendResponse { get; set; }
        public int PaymentMethodId { get; set; }
        public string StdToken { get; set; }
        public double WalletBalance {  get; set; }

        public double WalletAmountUsed{get; set; }

        public double FinalAmountToPay {  get; set; }
        public double Debt { get;set; }

        public string KeyChangeToken {  get; set; }

        public string BsstToken { get; set;}
        public string mrktMsg { get; set;}
        public string customerMsg {  get; set; }

        public string UnitNumber {  get; set; }
        public string Property {  get; set; }

        public string Estate { get; set; }
        public string PaymentMethod {  get; set; }
        public string ReceiptNumber { get; set; }

        public bool IsInHouseTransaction { get; set; }
        public string EFTRefNo { get; set; }
    }
}
