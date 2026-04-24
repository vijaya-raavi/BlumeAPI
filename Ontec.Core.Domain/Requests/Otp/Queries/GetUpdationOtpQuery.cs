using MediatR;
using Ontec.Core.Domain.Models.Dto.Otp;
using System.ComponentModel;

namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetUpdationOtpQuery : IRequest<OtpResponseModel>
    {
        public int UserId {  get; set; }
        public int CompanyId {  get; set; }
        public bool IsEmail {  get; set; }
        public string CountryCode { get; set; }

        [DisplayName("Mobile number")]
        public string? MobileNumber { get; set; }
        public string? EmailId { get; set; }
    }
}
