namespace Ontec.Core.Domain.Models.Dto.VendRequest
{
    public class STSVendRequestResponse
    {
        public int StatusCode { get; set; }

        public string TransactionDate {  get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string Token { get; set; }
        public string keyChangeToken { get; set; }
        public string bsstToken { get; set; }
        public string mrktMsg { get; set; }
        public string customerMsg { get; set; }
        public string ReceiptNumber { get; set; }
        public double DebtAmount {  get; set; }
    }
}
