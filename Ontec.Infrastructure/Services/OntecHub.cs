using Microsoft.AspNetCore.SignalR;
using Ontec.Core.Domain.Interface;

namespace Ontec.Infrastructure.Services
{
    public class OntecHub : Hub
    {
        private readonly IHubContext<OntecHub> _hubContext;
        private readonly ILiveUserService _liveUserService;
        public OntecHub(IHubContext<OntecHub> hubContext,ILiveUserService liveUserService)
        
        {
            _hubContext = hubContext;
            _liveUserService = liveUserService; ;
        }
        public string GetConnectionId() => Context.ConnectionId;

        public string GetConnectionUserId(string email) => Context.ConnectionId;

        public async Task NotifyAll()
            => await Clients.All.SendAsync("receivemessage");
        public async Task NotifyAppNotofication(string connectionId, string message)
            => await Clients.Client(Context.ConnectionId).SendAsync("receivemessage", message);

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var clientType = httpContext?.Request.Query["ClientType"].ToString()?.ToLower() ?? "web";
            var userId = Context.User?.Identity?.Name; // or get from claims

            _liveUserService.AddUser(Context.ConnectionId, userId, clientType);

            await Clients.All.SendAsync("LiveUserCountUpdated", _liveUserService.GetLiveUserCount());
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var clientType = httpContext?.Request.Query["ClientType"].ToString()?.ToLower() ?? "web";
            _liveUserService.RemoveUser(Context.ConnectionId);
            await Clients.All.SendAsync("LiveUserCountUpdated", _liveUserService.GetLiveUserCount());
            await base.OnDisconnectedAsync(exception);
        }


        //public override async Task OnConnectedAsync()
        //{
        //    await base.OnConnectedAsync();
        //    _liveUserService.AddUser(Context.ConnectionId);
        //    await Clients.All.SendAsync("LiveUserCountUpdated", _liveUserService.GetLiveUserCount());
        //}

        //public override async Task OnDisconnectedAsync(Exception? exception)
        //{
        //    _liveUserService.RemoveUser(Context.ConnectionId);
        //    await Clients.All.SendAsync("LiveUserCountUpdated", _liveUserService.GetLiveUserCount());
        //    await base.OnDisconnectedAsync(exception);
        //}
    }

}

