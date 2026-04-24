namespace Ontec.Core.Domain.Models.Dto.Wallet
{
    public class AddWalletTransactionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double TransactionAmount { get; set; }
        public string CreatedAt { get; set; }
        public string LastModfiedAt { get; set; }
        public string TransactionRemark { get; set; }
        public int TransactionTypeId { get; set; }
        public string TransactionDate { get; set; }
    }
}
