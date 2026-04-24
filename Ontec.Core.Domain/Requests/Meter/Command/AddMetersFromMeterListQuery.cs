using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class AddMetersFromMeterListQuery:IRequest<AddUpdateResultDto>
    {
        public int PropertyId { get; set; }
        public List<Meters> Meter { get; set; }

        public class Meters
        { 
            public string MeterNumber {  get; set; }
            public int MeterTypeId { get; set; }
            public double TargetConsumption {  get; set; }
        
        }
        
    }
}
