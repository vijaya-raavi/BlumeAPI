using FirebaseAdmin.Messaging;
using MediatR;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class UnsubscribeQueryRequest : IRequest<TopicManagementResponse>
    {
        public string Topic { get; set; }
    }

}
