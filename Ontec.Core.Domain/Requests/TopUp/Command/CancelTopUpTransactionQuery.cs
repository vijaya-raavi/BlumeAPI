using MediatR;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public  class CancelTopUpTransactionQuery : IRequest<CancelTransactionResponseModel>
    { 
        public string TransactionId { get; set; }
    }
}
