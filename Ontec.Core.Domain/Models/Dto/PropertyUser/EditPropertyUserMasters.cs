using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.PropertyUser
{
    public class EditPropertyUserMasters
    {
        public IEnumerable<OntecSelectListItem>? PropertiesList { get; set; }
        public IEnumerable<OntecSelectListItem>? TitleList { get; set; }
        public IEnumerable<OntecSelectListItem>? UserTypeList { get; set; }
    }
}
