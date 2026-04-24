using MediatR;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Property;

namespace Ontec.Core.Domain.Requests.Property.Handler
{
    public class GetAllPropertiesRequestQuery : BaseDatatableQuery<int?>, IRequest<DatatableModel<PropertyDto>>
    {
        public string? SearchText { get; set; }
        public int? StatusId { get; set; }
        public int? EstateId { get; set; }
    }
}
