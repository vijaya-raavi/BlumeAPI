using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class SaveUserSettingsQuery: IRequest<AddUpdateResultDto>
    {
     
        public int UserId { get; set; }
        public bool IsEmailEnabled {  get; set; }
        public bool IsMobileEnabled {  get; set; }
    }
}
