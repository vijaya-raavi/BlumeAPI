using MediatR;
using Ontec.Core.Domain.Models.Dto.Dashboard;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetAccountBalanceByPropertyIdQuery : IRequest<PropertyAccountBalanceDto>
    {
        public int PropertyId { get; set; } 
    }
}
