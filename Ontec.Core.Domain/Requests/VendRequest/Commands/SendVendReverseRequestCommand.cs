using MediatR;
using Ontec.Core.Domain.Models.Dto.VendRequest;

namespace Ontec.Core.Domain.Requests.VendRequest.Commands
{
    public class SendVendReverseRequestCommand : IRequest<VendRequestResponse>
    {
        public string PayType { get; set; }
        public decimal Amount { get; set; }
        public string TransactionNumber { get; set; }
        public string Meter { get; set; }
        public int NumTokens { get; set; }

        public SendVendReverseRequestCommand()
        {
            NumTokens = 1;
        }
    }
}
