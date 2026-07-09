using MediatR;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetCreditsByMeterIdQuery :IRequest<IEnumerable<UserCreditImageDto>>
    {
        public int MeterId {  get; set; }
    }
}
