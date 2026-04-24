using MediatR;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class GetUserSavedCardsQuery: IRequest<IEnumerable<GetUserCardsDto>>
    {
        public int UserId { get; set; }
    }
}
