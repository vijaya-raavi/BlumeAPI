using MediatR;
using Ontec.Core.Domain.Models.Dto.Otp;
using System.ComponentModel;

namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetRegisterOtpQuery : IRequest<OtpResponseModel>
    {
        public string CountryCode { get; set; }

        [DisplayName("Mobile number")]
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public int CompanyId { get; set; }
        public string Password {  get; set; }
    }
}
