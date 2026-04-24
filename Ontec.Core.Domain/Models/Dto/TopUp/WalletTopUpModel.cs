using Ontec.Core.Domain.Models.Dto.VendRequest;
using MediatR;
namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class WalletTopUpModel : IRequest<VendRequestResponse>
    {
        public string? m_payment_id { get; set; }
        public string? pf_payment_id { get; set; }
        public string? payment_status { get; set; }
        public string? merchant_id { get; set; }
        public string? signature { get; set; }
    }
}
