using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public  class RemoveConsumerFromGroupQueryRequest:IRequest<AddUpdateResultDto>
    {
        public int NotificationGroupLinkId {  get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
    }
}
