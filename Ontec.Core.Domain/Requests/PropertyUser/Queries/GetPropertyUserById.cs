using MediatR;
using Ontec.Core.Domain.Models.Dto.PropertyUser;

namespace Ontec.Core.Domain.Requests.PropertyUser.Queries
{
    public class GetPropertyUserById:IRequest<AddEditPropertyUser>
    {
        public int Id { get; set; }
    }
}