using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;
using System.ComponentModel;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class ChangePasswordQuery : IRequest<AddUpdateResultDto>
    {
        [DisplayName("User id")]
        public int UserId { get; set; }
        
        [DisplayName("Old password")]
        public string OldPassword { get; set; }
        
        [DisplayName("New password")]
        public string NewPassword { get; set; }
        
        [DisplayName("Confirm password")]
        public string ConfirmPassword { get; set; }

        public string SessionKey {  get; set; }
    }
}
