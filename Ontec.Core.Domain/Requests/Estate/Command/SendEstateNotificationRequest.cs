using MediatR;
using Ontec.Core.Domain.Models.Dto.Notification;

namespace Ontec.Core.Domain.Requests.Estate.Command
{
    public class SendEstateNotificationRequest:IRequest<NotificationGroupResponseModel>
    {
        public int EstateId { get; set; }
        public string Topic { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
