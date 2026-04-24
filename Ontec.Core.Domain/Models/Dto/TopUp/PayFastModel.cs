using MediatR;
using Ontec.Core.Domain.Models.Dto.VendRequest;

namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class PayFastModel : IRequest<VendRequestResponse>
    {
        public string? m_payment_id { get; set; }
        public string? pf_payment_id { get; set; }
        public string? payment_status { get; set; }
        public string? merchant_id { get; set; }
        public string? signature { get; set; }
        public bool? IsWalletRecharge { get; set; }=false;
    }
    
}

