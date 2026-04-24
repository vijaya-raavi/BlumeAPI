using Ontec.Core.Domain.Models.Dto.VendRequest;

namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public  class AddTopUpTransactionsDto 
    {
        
        public double Amount { get; set; }
        public int UserId { get; set; }
        public bool UseWallet { get; set; }
        public int MeterId { get; set; }
        public string TransactionID {  get; set; }
        public string TransactionFee { get; set; }
        public string RechargeAmount { get; set; }
        public string CreatedAt {  get; set; }
        public bool Flag {  get; set; }
        public double? WalletAmountUsed { get; set; } = 0;
        public TrailVendResponse TrailVendResponse { get; set;}

        public double FinalAmountToPay { get; set; }

        public string TrailVendResponseJson {  get; set; }
        public List<DebtItem> DebtItems { get; set; }
        public List<FixedItem> FixedItems { get; set; }
    }

}
