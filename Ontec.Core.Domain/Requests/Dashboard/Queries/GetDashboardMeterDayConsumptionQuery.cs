using MediatR;
using Ontec.Core.Domain.Models.Dto.Dashboard;
namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetDashboardMeterDayConsumptionQuery : IRequest<IEnumerable<DashboardMeterDayConsumptionDto>>
    {
        public int PropertyId { get; set; }
    }
}
