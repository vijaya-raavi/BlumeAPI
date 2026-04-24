namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class AddBankTransferTransactionDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public double Amount { get; set; }
        public int UserId { get; set; }
        public int UseWallet { get; set; }
        public int MeterId { get; set; }
        public double TransactionFee { get; set; }
        public double RechargeAmount { get; set; }
        public int Flag { get; set; }
        public string PfResponse { get; set; }
        public int PaymentMethodId { get; set; }
        public string TopupStatus { get; set; }
        public string VendResponse { get; set; }
        public string NetUpTransactionGuid { get; set; }
        public string Remark { get; set; }
        public string BankTransactionId { get; set; }
        public string EFTReferenceNumber { get; set; }

    }
    public class AddDeitecFailedTransactionDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public double Amount { get; set; }
        public string ReferenceNo { get; set; }

    }
}
