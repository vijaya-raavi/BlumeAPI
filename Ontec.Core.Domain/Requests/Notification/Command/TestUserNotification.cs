using MediatR;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class TestUserNotification:IRequest<bool>
    {
        public int UserId { get; set; }
    }
}
