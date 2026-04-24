using MediatR;
using Ontec.Core.Domain.Models.Dto.Consumption;

namespace Ontec.Core.Domain.Requests.Consumption.Queries
{
    public class GetConsumptionDashboardQuery : IRequest <ConsumptionDashboardDto>
    {
        public int PropertyId { get; set; }
        public string MeterId { get; set; }

        //public int? Year {  get; set; }
        public int ConsumptionCylceTypeId { get; set; }

        //public string ParentId { get; set; }
        //public string Level { get; set; }
        //public string? MonthStartDate { get; set; }
        //public int? WeekNumber { get; set; }
        //public bool? IsYearly {  get; set; }
    }
}
