using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateUserContactDeatilsRequestQuery:IRequest<AddUpdateResultDto>
    {
        public int UserId {  get; set; }
        public bool IsEmail {  get; set; }
        public string? CountryCode { get; set; }
        public int? CountryCodeId { get; set; }
        public string? MobileNumber { get; set; }
        public string? EmailId { get; set; }
        public string Otp { get; set; }
        public string OtpType { get; set; }
    }
}
