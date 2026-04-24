using MediatR;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class GetPaymentMethodsQuery: IRequest<IEnumerable<PaymentMethodsDto>>
    {
        public int StatusId { get; set; }
    }
}
