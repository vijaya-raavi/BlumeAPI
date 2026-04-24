using MediatR;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMeterExpiringListRequest : IRequest<IEnumerable<MeterExpiringDto>>
    {
        public int UserId { get; set; }
    }
}
