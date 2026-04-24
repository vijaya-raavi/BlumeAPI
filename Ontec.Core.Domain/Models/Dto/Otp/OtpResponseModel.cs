namespace Ontec.Core.Domain.Models.Dto.Otp
{
    public class OtpResponseModel
    {
        public string Type { get; set; }
        public string MobileResponse { get; set; }
        public string EmailResponse { get; set; }
        public string Otp { get; set; }
    }
}
