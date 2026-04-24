namespace Ontec.Core.Domain.Models.Dto.VendRequest
{
    public class VendReverseRequestResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string Code {  get; set; }
        public string Text {  get; set; }

        public string Token { get; set; }
        public string keyChangeToken { get; set; }
        public string bsstToken { get; set; }
        public string mrktMsg { get; set; }
        public string customerMsg { get; set; }
        public string ReceiptNumber { get; set; }
        public string Tarrif { get; set; }
        public string TxnDatetime { get; set; }
    }
}
