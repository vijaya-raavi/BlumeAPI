using MediatR;
using Ontec.Core.Domain.Models.Dto.Wallet;

namespace Ontec.Core.Domain.Requests.Wallet.Queries
{
    public  class GetUserWalletTransactionQuery : IRequest<IEnumerable<UserWalletTransactionDto>>
    {
        public int UserId {  get; set; }
    }
}
