using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.Company
{
    public class CompanyMasters
    {
        public IEnumerable<OntecSelectListItem>? CountryList { get; set; }
        public IEnumerable<OntecSelectListItem>? StateList { get; set; }
    }
}
