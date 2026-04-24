using Ontec.Core.Domain.Interface.Services;
using Ontec.Core.Domain.Interface.UserNotificationConnection;
using Ontec.Core.Domain.Requests.Notification.Queries;

namespace Ontec.Core.Application.Common.Helper
{
    public interface ICommonService
    {
        Task SendUserNotificationQuery(SendUserNotificationQuery request);
    }
    public class CommonService : ICommonService
    {
        private readonly IUserNotificationConnection _userNotificationConnection;
        private readonly ISignalRService _signalRService;
        public CommonService(IUserNotificationConnection userNotificationConnection
                               , ISignalRService signalRService)
        {
            _userNotificationConnection = userNotificationConnection;
            _signalRService = signalRService;
        }
        public async Task SendUserNotificationQuery(SendUserNotificationQuery request)
        {
            if (request.UserId != 0)
            {
                var connectionIds = await _userNotificationConnection.GetConnectionsByUserId(request.UserId).ConfigureAwait(false);
                foreach (var connection in connectionIds)
                {
                    await _signalRService.SendMessage(connection, request.Message);
                }
            }
        }
    }
}
