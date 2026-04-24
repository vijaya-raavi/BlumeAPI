using MediatR;
using Ontec.Core.Domain.Models.Dto.Transaction;

namespace Ontec.Core.Domain.Requests.Transaction.Queries
{
    public class GetTransactionMasterQuery : IRequest<TransactionMasterDto>
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        
    }

}
