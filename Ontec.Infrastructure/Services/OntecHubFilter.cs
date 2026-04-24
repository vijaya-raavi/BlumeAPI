using Microsoft.AspNetCore.SignalR;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.UserNotificationConnection;

namespace Ontec.Infrastructure.Services
{
    public class OntecHubFilter : IHubFilter
    {
        private readonly IWorkContext _workContext;
        private readonly IUserNotificationConnection _userNotificationConnection;
        public OntecHubFilter(IWorkContext workContext
                              , IUserNotificationConnection userNotificationConnection)
        {
            _workContext = workContext;
            _userNotificationConnection = userNotificationConnection;
        }

        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            try
            {
                if (invocationContext.HubMethodName.Equals("GetConnectionId", StringComparison.InvariantCultureIgnoreCase))
                {
                    //return await
                }
               
                return await next(invocationContext);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            _workContext.SetConnectionId(context.Context.ConnectionId);
            return next(context);
        }

        public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
        {
            //delete connectedId
            _userNotificationConnection.DeleteUserNotificationConnectionId(context.Context.ConnectionId);
            return next(context, exception);
        }
    }
}
