using MediatR;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class GetUserPayamenstQuery: BaseDatatableQuery<int?>, IRequest<AdminTopUpDto>
    {
        public string? SearchText { get; set; }
        public int EstateId {  get; set; }
        public bool IsPending { get; set; }
    }

    
}
