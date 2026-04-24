using MediatR;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class GetBankAccountsQuery : IRequest<IEnumerable<BankAccountDto>>
    {
        public int Id { get; set; }
    
    }
}
