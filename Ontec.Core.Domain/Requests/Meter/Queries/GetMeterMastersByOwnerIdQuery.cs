using MediatR;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMeterMastersByOwnerIdQuery : IRequest<EditMeterMasters>
    {
        public int OwnerId { get; set; }
    }
}
