using MediatR;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class DeleteNotificationByIdQuery:IRequest<string>
    {
        public IEnumerable<int> Ids { get; set; }
    }
}
