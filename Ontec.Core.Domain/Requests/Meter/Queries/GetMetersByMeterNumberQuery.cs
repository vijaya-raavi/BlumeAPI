using MediatR;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMetersByMeterNumberQuery:IRequest<IEnumerable<MetersUtilityDto>>
    {
        public string MeterNumber { get; set; }
        public int PropertyId { get; set; }
    }
}
