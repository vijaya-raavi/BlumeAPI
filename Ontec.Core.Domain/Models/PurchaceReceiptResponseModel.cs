using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models
{
    public class PurchaceReceiptResponseModel
    {
        public string ResponseMsg { get; set; }
        public string PurchaceReceiptUrl {  get; set; }
        public DocumentResultDto Receipt { get; set; }
    }
}
