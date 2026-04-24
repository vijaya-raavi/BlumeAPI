using MediatR;
using Ontec.Core.Domain.Models.Dto.PropertyUser;

namespace Ontec.Core.Domain.Requests.PropertyUser.Queries
{
    public class GetPropertyUserListByPropertyIdQuery: IRequest<PropertyUserDto>
    {
        public int PropertyId { get; set; }
    }
}
