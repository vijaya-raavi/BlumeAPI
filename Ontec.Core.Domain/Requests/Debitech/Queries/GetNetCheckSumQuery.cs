using MediatR;
using Ontec.Core.Domain.Requests.Debitech.Command;

namespace Ontec.Core.Domain.Requests.Debitech.Queries
{
    public class GetNetCheckSumQuery: BankNotificationRequest, IRequest<string>
    {
        
    }
}
