using MediatR;
using Ontec.Core.Domain.Models.Dto;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetAdminDashboardMasterQuery: IRequest<AdminDashboardMastersDto>
    {
        public int UserId { get; set; }
        public int CompanyId {  get; set; }
    }
}
