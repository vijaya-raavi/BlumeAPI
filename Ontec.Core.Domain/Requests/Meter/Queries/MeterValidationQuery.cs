using MediatR;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class MeterValidationQuery : IRequest<bool>
    {
        public string MeterNumber { get; set; }
    }
}
