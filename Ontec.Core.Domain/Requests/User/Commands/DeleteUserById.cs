using MediatR;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class DeleteUserById : IRequest<string>
    {
        public int Id { get; set; }
    }
}
