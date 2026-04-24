using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Notification.Queries
{
    public class GetGroupForNotificationQuery:IRequest<IEnumerable<OntecSelectListItem>>
    {
    }
}
