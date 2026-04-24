using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.Notification
{
    public class NotificationTypeMasterDto
    {
        public IEnumerable<OntecSelectListItem>? NotificationTypeList { get; set; }
    }
}
