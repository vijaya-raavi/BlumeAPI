using MediatR;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public class DeletePropertyUserById : IRequest<string>
    {
        public int Id { get; set; }
    }
}