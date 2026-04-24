using MediatR;
using Ontec.Core.Domain.Models;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class UpdateUserConnectionIdQuery : IRequest<int>
    {
        public string OldConnectionId { get; set; }
        public string NewConnectionId { get; set; }
        public int UserId { get; set; }
        public string ClientType {  get; set; }
    }
}
