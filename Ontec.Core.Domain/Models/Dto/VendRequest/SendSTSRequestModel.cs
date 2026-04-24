namespace Ontec.Core.Domain.Models.Dto.VendRequest
{
    public class SendSTSRequestModel
    {
        public string Meter { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
