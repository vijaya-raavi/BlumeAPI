using MediatR;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMeterRequestQuery: BaseDatatableQuery<int?>, IRequest<DatatableModel<MeterRequestDto>>
    {
        //public int RoleId { get; set; }
        //public bool? IsActive { get; set; }
        public string? SearchText { get; set; }
    }
}
