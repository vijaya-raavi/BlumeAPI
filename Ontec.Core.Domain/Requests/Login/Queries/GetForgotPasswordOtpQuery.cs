using MediatR;
using Ontec.Core.Domain.Models.Dto.Otp;

namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetForgotPasswordOtpQuery : IRequest<OtpResponseModel>
    {
        public string CountryCode { get; set; }
        public string EmailMobile { get; set; }
        public int CompanyId { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}
