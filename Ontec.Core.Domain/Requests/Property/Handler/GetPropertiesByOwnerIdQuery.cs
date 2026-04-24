using MediatR;
using Ontec.Core.Domain.Models.Dto.Property;

namespace Ontec.Core.Domain.Requests.Property.Handler
{
    public class GetPropertiesByOwnerIdQuery : IRequest<IEnumerable<PropertyDto>>
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public bool? InActive { get; set; }
    }
}
