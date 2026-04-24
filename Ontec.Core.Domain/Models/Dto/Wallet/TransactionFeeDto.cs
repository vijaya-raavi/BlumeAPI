namespace Ontec.Core.Domain.Models.Dto.Wallet
{
    public  class TransactionFeeDto
    {
        public double Amount { get; set; }
        public double TopUpAmount {  get; set; }
        public double TransactionFee { get; set; }
        public double DiscountFee {  get; set; }
        public double FinalAmountToPay {  get; set; }
        public double WalletAmountUsed {  get; set; }
    }
}
