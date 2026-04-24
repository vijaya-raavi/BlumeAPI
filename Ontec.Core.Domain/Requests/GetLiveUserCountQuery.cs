using MediatR;
using Ontec.Core.Domain.Models.Dto;

namespace Ontec.Core.Domain.Requests
{
    public  class GetLiveUserCountQuery:IRequest<LiveUserCountDto>
    {
    }
}
