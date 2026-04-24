namespace Ontec.Core.Domain.Models.Dto.AccountTransactions
{
    public class AccountTransactionApiModel
    {
        public List<AccountTransaction> Data { get; set; }
    }
    public class AccountTransaction
    {
        public string Id { get; set; }
        public string CustomerAgreementId { get; set; }
        public string AccountTransType { get; set; }
        public string CustomerTransId { get; set; }
        public DateTime TransDate { get; set; }
        public DateTime DateEntered { get; set; }
        public string AccountRef { get; set; }
        public string OurRef { get; set; }
        public decimal AmtInclTax { get; set; }
        public decimal AmtTax { get; set; }
        public decimal ResultantBalance { get; set; }
        public string UserRecEntered { get; set; }
        public string Comment {  get; set; }
        public string Tariff {  get; set; }
    }
}
