using MediatR;
using Ontec.Core.Domain.Models.Dto.VendRequest;

namespace Ontec.Core.Domain.Requests.VendRequest.Commands
{
    public class SendVendSTSRequestCommand : IRequest<IEnumerable<STSVendRequestResponse>>
    {
        public string Meter { get; set; }
        public DateTime FromDate {  get; set; }
        public DateTime ToDate { get; set; }
    }

    
}
