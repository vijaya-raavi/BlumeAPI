using MediatR;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Common.Queries
{
    public class GetAuditTrailQuery : BaseDatatableQuery<int?>, IRequest<DatatableModel<AuditTrailDto>>
    {
        public string? SearchText { get; set; }
        public int? UserId { get; set; }
        public DateTime FromDate {  get; set; }
        public DateTime ToDate { get; set; }
    }
}
