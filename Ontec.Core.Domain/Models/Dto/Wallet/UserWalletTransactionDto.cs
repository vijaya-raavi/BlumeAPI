namespace Ontec.Core.Domain.Models.Dto.Wallet
{
    public class UserWalletTransactionDto
    {
        public string TransactionId { get; set; }
        public double TransactionAmount {  get; set; }

        //public double UpdatedWalletBalance { get; set; }
        public string TransactionType { get; set; }
            
        public string TransactionRemark { get; set; }

        public string TransactionDate {  get; set; }


    }
}
