using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models;
using MediatR;

namespace Ontec.Core.Domain.Requests.Operator.Queries
{
    public class GetOperatorsQuery : BaseDatatableQuery<int?>, IRequest<DatatableModel<OperatorDto>>
    {
        public int RoleId { get; set; }
        public bool? IsActive { get; set; }
        public int? Id {  get; set; }

    }
}
