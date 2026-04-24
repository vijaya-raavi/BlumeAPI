namespace Ontec.Core.Domain.Models.Dto.Otp
{
    public class OtpModel:BaseModel
    {
        public string Otp { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public int StatusId { get; set; }
        public int CompanyId { get; set; }
        public int CountryCodeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
