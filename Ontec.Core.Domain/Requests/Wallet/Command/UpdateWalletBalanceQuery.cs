using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Wallet.Command
{
    public class UpdateWalletBalanceQuery : IRequest<AddUpdateResultDto>
    {
        // public int Id { get; set; }
        public int UserId { get; set; }
        public double Amount { get; set; }//amount;
    }
}
