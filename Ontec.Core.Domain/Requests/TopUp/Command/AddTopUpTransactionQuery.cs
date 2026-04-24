using MediatR;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public class AddTopUpTransactionQuery:IRequest<AddTopUpTransactionsDto>
    {

        public int Id { get; set; }
        public double Amount {  get; set; }
        public int UserId {  get; set; }
        public bool UseWallet { get; set; }
        public int MeterId {  get; set; }  
        
        public int PaymentMethodId {  get; set; }
      
        public bool flag {  get; set; }
    }
}
