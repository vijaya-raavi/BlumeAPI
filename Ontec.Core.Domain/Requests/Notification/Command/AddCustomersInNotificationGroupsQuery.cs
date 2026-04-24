using MediatR;
using Ontec.Core.Domain.Models.Dto.Notification;

namespace Ontec.Core.Domain.Requests.Notification.Commands
{
    public class AddCustomersInNotificationGroupsQuery : IRequest<NotificationGroupResponseModel>
    {
        public int GroupId {  get; set; }
        public IEnumerable<int> UserIds { get; set; }

    }
}
