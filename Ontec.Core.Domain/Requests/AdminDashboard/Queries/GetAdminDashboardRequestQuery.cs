using MediatR;
using Ontec.Core.Domain.Models.Dto.AdminDashboard;

namespace Ontec.Core.Domain.Requests.AdminDashboard.Queries
{
    public class GetAdminDashboardRequestQuery:IRequest<AdminDashboardDto>
    {
    }
}
