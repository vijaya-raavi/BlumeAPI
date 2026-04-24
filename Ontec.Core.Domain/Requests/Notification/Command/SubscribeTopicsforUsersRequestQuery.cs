using MediatR;
using Ontec.Core.Domain.Models.Dto.Debitech;
using Ontec.Core.Domain.Models.Dto.Notification;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class SubscribeTopicsforUsersRequestQuery:IRequest<NotificationGroupResponseModel>
    {
        public int GroupId { get; set; }
        public IEnumerable<int> UserIds { get; set; }
    }
}
