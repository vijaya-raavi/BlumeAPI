using Ontec.Core.Domain.Requests.Notification.Command;

namespace Ontec.Core.Domain.Interface.UserNotificationConnection
{
    public interface IUserNotificationConnection
    {
        Task<int> UpdateUserNotificatioConnection(UpdateUserConnectionIdQuery request);
        Task<int> InsertUserNotificationConnection(string connectionId, int userId,string clientType);
        Task DeleteUserNotificationConnectionId(string connectionId);
        Task<IEnumerable<string>> GetConnectionsByUserId(int userId);
        Task<string> GetUserConnectionId(int userId);
    }
}
