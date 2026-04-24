using MediatR;
using Ontec.Core.Domain.Models.Dto.Consumer;

namespace Ontec.Core.Domain.Requests.Consumer.Queries
{
    public class GetConsumerMastersQuery : IRequest<ConsumerMasterDto>
    {
        public int EstateId {  get; set; }
    }
}
