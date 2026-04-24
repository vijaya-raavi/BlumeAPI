using MediatR;
using Ontec.Core.Domain.Models.Dto.Notification;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class SendGroupNotificationRequest: IRequest<NotificationGroupResponseModel>
    {
        public List<int> UserIds { get; set; }
        public int GroupId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
