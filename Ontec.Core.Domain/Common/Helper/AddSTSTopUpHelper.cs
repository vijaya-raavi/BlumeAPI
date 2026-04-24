namespace Ontec.Core.Domain.Common.Helper
{
    public class AddSTSTopUpHelper
    {
        public string TransactionId { get; set; }

        public int UserId { get; set; }
        public int MeterId { get; set; }
        public string StdToken { get; set; }
        public double DebtAmount { get; set; }
        public string KeyChangeToken { get; set; }
        public string BsstToken { get; set; }
        public string MrktMsg { get; set; }
        public string CustomerMsg { get; set; }
        public string RCTNumber { get; set; }
        public string vendResponse {  get; set; }
        public string TarrifUnits{get;set;}
        public string Message {  get; set; }

        public string TxnDate { get; set; }
    }
}
