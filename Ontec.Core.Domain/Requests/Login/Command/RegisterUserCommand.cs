using MediatR;
using Ontec.Core.Domain.Models.Dto.Login;

namespace Ontec.Core.Domain.Requests.Login.Command
{
    public class RegisterUserCommand : IRequest<LoginResult>
    {
        public string CountryCode { get; set; }
        public int CountryCodeId { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public int CompanyId { get; set; }
        public string Otp { get; set; }
        public string OtpType { get; set; }
        public bool Isbusiness {  get; set; }
        public string? Devicetoken {  get; set; } 
    }
}
