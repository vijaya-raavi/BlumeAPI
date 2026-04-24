using MediatR;
using Ontec.Core.Domain.Models.Dto.Account;

namespace Ontec.Core.Domain.Requests.Account.Queries
{
    public class GetAccountHisotryQuery :IRequest<AccountBalanceDto>
    {
        public int PropertyId { get; set; }
    }
}
