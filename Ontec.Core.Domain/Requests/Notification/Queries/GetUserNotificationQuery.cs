using MediatR;

namespace Ontec.Core.Domain.Requests.Notification.Queries
{
    public class SendUserNotificationQuery : IRequest<bool>
    {
        public int UserId { get; set; }
        public string Message { get; set; }  
    }
}
