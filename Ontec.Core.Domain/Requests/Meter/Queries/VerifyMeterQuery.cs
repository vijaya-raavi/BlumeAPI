using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class VerifyMeterQuery : IRequest<AddUpdateResultDto>
    {
        public int PropertyId { get; set; }
        public string MeterNumber { get; set; }
    }
}
