using MediatR;
using Ontec.Core.Domain.Models.Dto.Notification;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class SendGroupNotifications:IRequest<string>
    {

        public int GroupId { get; set; }
        public string Title {  get; set; }
        public string Body {  get; set; }
    }
}
