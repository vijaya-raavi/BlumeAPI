using MediatR;
using Ontec.Core.Domain.Models.Dto.Dashboard;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetDashboardMastersQuery : IRequest<IEnumerable<DashboardMastersDto>>
    {
        public int UserId { get; set; }
        public int CompanyId {  get; set; }
    }
}
