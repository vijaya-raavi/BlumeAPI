using MediatR;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.Common.Queries;

namespace Ontec.Core.Application.AuditTrail.Command
{
    public class AuditTrailCommandHandler : IRequestHandler<GetAuditTrailQuery, DatatableModel<AuditTrailDto>>
    {
        private readonly IAuditTrail _auditTrail;
        public AuditTrailCommandHandler(IAuditTrail auditTrail)
        {
            _auditTrail = auditTrail;
        }
        #region GetAuditTrail
        public async Task<DatatableModel<AuditTrailDto>> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken)
        {
            return await _auditTrail.GetAuditTrail(request);
        }
        #endregion
    }
}
