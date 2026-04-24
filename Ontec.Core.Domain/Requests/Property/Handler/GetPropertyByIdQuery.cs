using MediatR;
using Ontec.Core.Domain.Models.Dto.Property;

namespace Ontec.Core.Domain.Requests.Property.Handler
{
    public class GetPropertyByIdQuery : IRequest<PropertyModelDto>
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
}
