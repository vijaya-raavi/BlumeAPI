using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.Common.Queries;

namespace Ontec.Core.Domain.Common.Helper
{
    public interface IAuditTrail
    {
        Task<int> AuditTrail(AuditHelper auditHelper);

        Task<DatatableModel<AuditTrailDto>> GetAuditTrail(GetAuditTrailQuery request);
    }
}
