using MediatR;
using Ontec.Core.Domain.Models.Dto.Wallet;

namespace Ontec.Core.Domain.Requests.Wallet.Queries
{
    public class GetUserWalletQuery : IRequest<UserWalletDto>
    {
        public int UserId { get; set; }
    }
}
