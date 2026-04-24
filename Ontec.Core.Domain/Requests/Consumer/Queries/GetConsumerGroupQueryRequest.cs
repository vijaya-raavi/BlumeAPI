using MediatR;
using Ontec.Core.Domain.Models.Dto.Consumer;

namespace Ontec.Core.Domain.Requests.Consumer.Queries
{
    public class GetConsumerGroupQueryRequest:IRequest<IEnumerable<ConsumerGroupDto>>
    {
        public int UserId {  get; set; }
    }
}
