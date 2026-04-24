using Ontec.Core.Domain.Models.Dto.Charts;

namespace Ontec.Core.Domain.Models.Dto.Account
{
    public class AccountBalanceDto
    {
        public decimal AccountBalance { get; set; }
        public string AccountName { get; set; }
        public LineChartDto AccountHistory { get; set; }
        public List<RececntTransaction> RececntTransactions { get; set; }
    }
    public class RececntTransaction
    { 
      public string TransactionId {  get; set; }
      public decimal Amount {  get; set; }
      public DateTime TransactionDate {  get; set; }
     public string TransactionType { get; set; }

    }
}
