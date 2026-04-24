using MediatR;
using Ontec.Core.Domain.Models.Dto.PropertyUser;

namespace Ontec.Core.Domain.Requests.PropertyUser.Queries
{
    public class GetPropertyUserMastersByOwnerIdQuery:IRequest<EditPropertyUserMasters>
    {
        public int OwnerId { get; set; }
    }
}
