using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.MasteUserAccount;

namespace Ontec.Core.Domain.Models.Dto.Account
{
    public class AccountBalanceDto
    {
        public decimal AccountBalance { get; set; }
        public decimal AuxAccountBalance { get; set; }
        public string AccountName { get; set; }
        public LineChartDto AccountHistory { get; set; }

        public int TotalAuxAccounts { get; set; }

        public List<RececntTransaction> RececntTransactions { get; set; }

        public List<AuxAccountDto> AuxAccounts { get; set; } = new List<AuxAccountDto>();
    }
    public class RececntTransaction
    { 
      public string TransactionId {  get; set; }
      public decimal Amount {  get; set; }
      public DateTime TransactionDate {  get; set; }
     public string TransactionType { get; set; }

    }
}
