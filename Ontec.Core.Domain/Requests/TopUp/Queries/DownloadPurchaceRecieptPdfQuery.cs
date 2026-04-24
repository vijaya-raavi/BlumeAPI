using MediatR;
using Ontec.Core.Domain.Models;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class DownloadPurchaceRecieptPdfQuery : IRequest<PurchaceReceiptResponseModel>
    {
        public string TransactionId { get; set; }
    }
}
