using MediatR;

namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class DeleteMeterById : IRequest<string>
    {
        public int Id { get; set; }
    }
}
