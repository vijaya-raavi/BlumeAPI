using MediatR;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public class DeleteConsumerById :IRequest<string>
    {
        public int Id { get; set; }
    }
}
