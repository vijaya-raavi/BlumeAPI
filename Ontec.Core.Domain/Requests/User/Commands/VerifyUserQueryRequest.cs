using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class VerifyUserQueryRequest:IRequest<AddUpdateResultDto>
    {
        public int UserId {  get; set; }
    }
}
