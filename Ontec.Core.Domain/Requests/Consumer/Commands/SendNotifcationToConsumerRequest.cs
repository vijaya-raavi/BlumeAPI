using MediatR;
using Ontec.Core.Domain.Models.Dto;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public  class SendNotifcationToConsumerRequest :IRequest<PushNotificationDto>
    {
        public int ConsumerId {  get; set; }
        public string Title {  get; set; }
        public string Body {  get; set; }
    }
}
