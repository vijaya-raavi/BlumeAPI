namespace Ontec.Core.Domain.Models.Dto.VendRequest
{
    public class VendRequestResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string Token { get; set; }
        public string keyChangeToken {  get; set; }
        public string bsstToken {  get; set; }
        public string mrktMsg {  get; set; }
        public string customerMsg {  get; set; }
        public string ReceiptNumber {  get; set; }
        public string Tarrif {  get; set; }
        public string TxnDatetime {  get; set; }

        public string VendReference { get; set; }

    }

    public class TrailVendResponse : VendRequestResponse
    {
        public string Units { get; set; }
        public double Amount { get; set; }
        public double Tax { get; set; }
        public string Tariff { get; set; }
        public double Debt { get; set; }
        public string DebtDescription { get; set; }
    }
}
