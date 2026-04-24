using MediatR;
using Ontec.Core.Domain.Models.Dto.Estate;

namespace Ontec.Core.Domain.Requests.Estate.Queries
{
    public class GetEstatesRequestQuery :IRequest<IEnumerable<EstateDto>>
    {
    }
}
