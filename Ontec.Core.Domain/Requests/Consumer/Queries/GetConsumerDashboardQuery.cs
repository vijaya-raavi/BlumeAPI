using MediatR;
using Ontec.Core.Domain.Models.Dto.User;

namespace Ontec.Core.Domain.Requests.Consumer.Queries
{
    public class GetConsumerDashboardQuery: IRequest<ConsumerDashboardDto>
    {
        //public int RoleId {  get; set; }
    }
}
