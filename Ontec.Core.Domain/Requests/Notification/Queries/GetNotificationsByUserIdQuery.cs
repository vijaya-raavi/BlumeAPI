using MediatR;
using Ontec.Core.Domain.Models.Dto.Notification;

namespace Ontec.Core.Domain.Requests.Notification.Queries
{
    public class GetNotificationsByUserIdQuery : IRequest<NotificationsDto>
    {
        public string ConnectionId { get; set; }

        public int NotificationTypeId {  get; set; }
        public int UserId { get; set; }
        public bool IsShortView { get; set; }

        public string NoteStatus {  get; set; }
    }
}
