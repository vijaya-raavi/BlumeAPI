using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.Consumer
{
    public class ConsumerMasterDto
    {
        public IEnumerable<OntecSelectListItem> StatusList { get; set; }
    }

    public class ConsumerStatusCountDto
    {
        public int StatusId { get; set; }
        public int UserCount { get; set; }
    }

}
