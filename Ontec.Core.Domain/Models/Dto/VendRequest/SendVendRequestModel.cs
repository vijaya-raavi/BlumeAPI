namespace Ontec.Core.Domain.Models.Dto.VendRequest
{
    public class SendVendRequestModel
    {
        public string PayType { get; set; }
        public double Amount { get; set; }
        public string TransactionNumber { get; set; }
        public string Meter { get; set; }
        public int NumTokens { get; set; }

        public SendVendRequestModel()
        {
            NumTokens = 1;
        }
    }
}
