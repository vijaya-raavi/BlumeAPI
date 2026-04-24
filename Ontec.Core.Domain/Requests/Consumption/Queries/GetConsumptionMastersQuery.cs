using MediatR;
using Ontec.Core.Domain.Models.Dto.Consumption;

namespace Ontec.Core.Domain.Requests.Consumption.Queries
{
    public class GetConsumptionMastersQuery:IRequest<ConsumptionMastersDto>
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int ConsumerId {  get; set; }
    }
}
