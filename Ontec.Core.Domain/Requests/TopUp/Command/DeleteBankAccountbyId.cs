using MediatR;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public class DeleteBankAccountbyId :IRequest<string>
    {
        public int Id { get; set; }
    }
}
