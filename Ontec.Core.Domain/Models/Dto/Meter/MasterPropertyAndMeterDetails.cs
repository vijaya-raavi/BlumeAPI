using Ontec.Core.Domain.Models.Dto.Consumption;

namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class MasterPropertyAndMeterDetails
    {
        public Customer Customer { get; set; }
        public IEnumerable<MetersUtilityDto> MeterDetails { get; set; }
    }
}
