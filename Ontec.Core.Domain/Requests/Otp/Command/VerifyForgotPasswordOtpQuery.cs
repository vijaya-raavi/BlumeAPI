using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Otp.Command
{
    public class VerifyForgotPasswordOtpQuery:IRequest<VerifyOTPDto>
    {
        public string OTP { get; set; }
        public string EmailMobile { get; set; }
        public int CompanyId { get; set; }
    }
}
