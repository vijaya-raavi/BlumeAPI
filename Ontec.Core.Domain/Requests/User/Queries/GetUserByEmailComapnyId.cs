using MediatR;
using Ontec.Core.Domain.Models;

namespace Ontec.Core.Domain.Requests.User.Queries
{
    public class GetUserByEmailComapnyId : IRequest<UserDto>
    {
        public string Email { get; set; }
        public int CompanyId { get; set; }
    }
}
