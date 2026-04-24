using MediatR;

namespace Ontec.Core.Domain.Requests.Operator.Command
{
    public class DeleteOperatorUserById : IRequest<string>
    {
        public int Id { get; set; }
    }
}
