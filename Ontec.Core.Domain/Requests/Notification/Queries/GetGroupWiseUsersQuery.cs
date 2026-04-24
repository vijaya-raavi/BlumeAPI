using MediatR;
using Ontec.Core.Domain.Models.Dto.Notification;

namespace Ontec.Core.Domain.Requests.Notification.Queries
{
    public  class GetGroupWiseUsersQuery :IRequest<IEnumerable<GroupWiseDetailedDto>>
    {
        //public int? GroupId {  get; set; }
    }
}
