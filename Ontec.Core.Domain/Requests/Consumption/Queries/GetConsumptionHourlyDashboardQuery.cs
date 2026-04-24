using MediatR;
using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.Consumption;

namespace Ontec.Core.Domain.Requests.Consumption.Queries
{
    public class GetConsumptionHourlyDashboardQuery : IRequest<LineChartDto>
    {
        public int PropertyId { get; set; }
        public string MeterId { get; set; }
        public DateTime Date {  get; set; }
        
    }
   
}

