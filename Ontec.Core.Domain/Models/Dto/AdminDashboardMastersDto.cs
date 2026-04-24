using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto
{
    public class AdminDashboardMastersDto
    {
        public IEnumerable<Utilities>? UtilityTypes { get; set; }
        public IEnumerable<OntecSelectListItem>? Period { get; set; }
    }
    public class Utilities
    {
        public int id { get; set; }
        public string name { get; set; }
        //public string  UnitOfMeasure { get; set; }
        //public string MinDailyTarget {  get; set; }
        //public string MaxDailyTarget {  get; set; }
    }
}
