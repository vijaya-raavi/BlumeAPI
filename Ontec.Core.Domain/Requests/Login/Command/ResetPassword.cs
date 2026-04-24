using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Login.Command
{
    public class ResetPassword: IRequest<AddUpdateResultDto>
    {
        public string EmailMobile { get; set; }
        public string CountryCode { get; set; }
        public int CountryCodeId { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public int CompanyId { get; set; }
        public string LastOtp { get; set; }
        public bool IsAdmin { get; set; } = false;

        public string VerifiedKey {  get; set; }
    }
}
