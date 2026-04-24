using MediatR;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public  class GetTopUpTransactionsQuery : BaseDatatableQuery<int?> ,IRequest<DatatableModel<GetTopUpTransaction>>
    {
        public string? TransactionId {  get; set; }
        public int? UserId { get; set; }
        public string? SearchText {  get; set; } 
    }
}
