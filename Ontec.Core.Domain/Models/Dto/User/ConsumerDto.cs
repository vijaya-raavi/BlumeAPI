using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models.Dto.User
{
    public class ConsumerDto : BaseModel
    {
        public string ProfileUrl { get; set; }
        public string Consumer { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Sources { get; set; }
        public string meter_numbers { get; set; }
        public string CreateDate { get; set; }
        public int Status { get; set; }
        public string DocumentUrl { get; set; }
        public string DocNumber { get; set; }
        public string DocType { get; set; }
        public int DocTypeId { get; set; }
        public double WalletBalance {  get; set; }
        public bool IsVerified { get; set; }
        public DocumentResultDto Document { get; set; }
        public DocumentResultDto Profile { get; set; }
        public string VATNumber {  get; set; }
    }
    public class PropertyCountDto { public int UserId; public int PropertyCount; }
    public class MeterCountDto { public int UserId; public int MeterCount; }
    public class MeterNumberDto { public int UserId; public string MeterNumbers; }
}
