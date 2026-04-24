using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.Consumption
{
    public class ConsumptionMastersDto
    {
        public IEnumerable<ConsumptionPropertyList>? PropertyList { get; set; }
        public IEnumerable<OntecSelectListItem>? ConsumptionCylceList { get; set; }
    }
    public class ConsumptionPropertyList
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string UnitNumber { get; set; }
        public List<MeterSelectList> MeterList { get; set; }
    }
    public class MeterSelectList
    {
        public string MeterId { get; set; }
        public string Name { get; set; }
        public string UtilityType { get; set; }
    }
}
