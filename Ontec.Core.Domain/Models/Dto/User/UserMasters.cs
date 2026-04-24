using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.User
{
    public class UserMasters
    {
        public IEnumerable<OntecSelectListItem>? StatusList { get; set; }
        public IEnumerable<OntecSelectListItem>? CommunicationTypeList { get; set; }
        public IEnumerable<OntecSelectListItem>? RoleMasterList { get; set; }
        public IEnumerable<OntecSelectListItem>? TitleList { get; set; }
        public IEnumerable<OntecSelectListItem>? ProofDocumentTypeList { get; set; }
    }
}
