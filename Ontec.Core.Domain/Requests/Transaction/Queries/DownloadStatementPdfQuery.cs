using MediatR;
using Ontec.Core.Domain.Models;

namespace Ontec.Core.Domain.Requests.Transaction.Queries
{
    public class DownloadStatementPdfQuery: IRequest<TransactionStatementResponseModel>
    {
       public int PropertyId { get; set; }

        //public int MeterId { get; set; }
        public int TransactionCycleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime Todate { get; set; }

        public string Type {  get; set; }
    
    }
}
