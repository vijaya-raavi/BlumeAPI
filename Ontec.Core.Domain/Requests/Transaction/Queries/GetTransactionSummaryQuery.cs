using MediatR;
using Ontec.Core.Domain.Models.Dto.Transaction;

namespace Ontec.Core.Domain.Requests.Transaction.Queries
{
    public class GetTransactionSummaryQuery : IRequest<TransactionSummaryDto>
    {
        public int PropertyId { get; set; }
        public int TransactionCycleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime Todate { get; set; }
    }
}
