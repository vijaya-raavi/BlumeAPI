using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models.Dto.Configuration
{
    public  class Configurations
    {
        public IEnumerable <ConfigurationDto>configurations { get; set; }
        public IEnumerable <BusinessHoursConfigurationsDto> businessHoursConfigurations { get; set; }
        public IEnumerable<MeterTypes> MeterUtilityTypes { get; set; }
    }
}
