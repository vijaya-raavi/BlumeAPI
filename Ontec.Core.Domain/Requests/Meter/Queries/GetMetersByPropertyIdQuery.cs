using MediatR;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMetersByPropertyIdQuery : IRequest<PropertyMetersDto>
    {
        public int Id { get; set; }
    }
}
