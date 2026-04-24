using MediatR;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Requests;

namespace Ontec.Core.Application.LiveUserCount
{
    public class GetLiveUserCountHandler : IRequestHandler<GetLiveUserCountQuery, LiveUserCountDto>
    {
        private readonly ILiveUserService _liveUserService;

        public GetLiveUserCountHandler(ILiveUserService liveUserService)
        {
            _liveUserService = liveUserService;
        }

        public Task<LiveUserCountDto> Handle(GetLiveUserCountQuery request, CancellationToken cancellationToken)
        {
            var count = _liveUserService.GetLiveUserCount();
            return Task.FromResult(count);
        }
    }
}
