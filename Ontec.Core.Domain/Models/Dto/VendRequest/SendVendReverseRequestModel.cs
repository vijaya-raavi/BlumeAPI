namespace Ontec.Core.Domain.Models.Dto.VendRequest
{
    public class SendVendReverseRequestModel
    {
        public string OriginRef { get; set; }
        public string OriginTime { get; set; }
        public string TransactionNumber { get; set; }
        public int RepeatCount { get; set; }

    }
}
