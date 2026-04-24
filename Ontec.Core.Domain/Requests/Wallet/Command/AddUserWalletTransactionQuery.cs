using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Wallet.Command
{
    public  class AddUserWalletTransactionQuery : IRequest<AddUpdateResultDto>
    {
        public int WalletId { get; set; }
        public string TransactionId {  get; set; }
        public double TransactionAmount {  get; set; }
        public int TransactionType {  get; set; }
        public string TransactionRemark {  get; set; }
        public double UpdatedBalance {  get; set; }
        public string Transactiondate {  get; set; }
    }
}
