using MediatR;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.Purchase.Queries
{
    public class GetSTSPurchaseDetails:IRequest<GetSTSTopUpTransactions>
    {
        public string Meter {  get; set; }

        public int StsTransactionPeriod {  get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
