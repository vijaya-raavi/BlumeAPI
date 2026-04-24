using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Notification;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.Notification.Commands;
using Ontec.Core.Domain.Requests.Notification.Queries;

namespace Ontec.Core.Domain.Interface.Notifiation
{
    public interface INotificationRepository
    {
        Task<IEnumerable<OntecSelectListItem>> GetNotificationTypeMasters();
        Task<int> AddNotifications(AddOrUpdateNotificationsQuery request);
        Task<int> UpdateNotifications(int notificationId, string meterNumber, string noteStatus);
        Task<bool> IsNotificationTypeExist(int id);
        Task<int> IsNotificationsExist(int id);
        Task<NotificationsDto> GetNotificationsByUserId(GetNotificationsByUserIdQuery request);
        Task DeleteNotificationById(IEnumerable<int> ids);
        Task<int> GetNotificationId(string meterNumber,int notificationTypeId);
        Task<int> AddNotificationGroup(AddEditGroupQuery request);
        Task<int> UpdateNotificationGroup(AddEditGroupQuery request);
        Task<IEnumerable<OntecSelectListItem>> GetGroups();
        Task<IEnumerable<OntecSelectListItem>> GetUsers();
        Task<int> IsGroupIdExist(int id);
        Task<int> IsGroupExist(string group);
        Task<int> InsertGroupUsers(int groupId, IEnumerable<int> UserIds, string notificationKey);
        Task<int> AddCustomersInNotificationGroups(AddCustomersInNotificationGroupsQuery request,string notificationKey);
        Task UpdateGroupUsers(IEnumerable<int> userId, int groupId, IEnumerable<int> existUserIds);
        Task<IEnumerable<GroupWiseUsersDto>> GetGroupWiseUsers(GetGroupWiseUsersQuery request);
        Task<int> IsNotifiationKeyExist(string key);
        Task<int> InsertTopicUsers(int groupId, IEnumerable<int> UserIds);
        Task<int> AddCustomersInNotificationTopics(SubscribeTopicsforUsersRequestQuery request);
        Task<int> GetGroupIdByEstateId(int estateId);
        Task<int> IsInActiveGroupExist(string group);
        Task<int> ActiveNotificationGroup(AddEditGroupQuery request);
        Task DeleteUserFromNotifications(int userId);
        Task<IEnumerable<GroupWiseUsersDto>> GetGroupWiseUsersByGroupId(int id);
        Task<int> InsertTopicUser(int groupId, int userId);
        Task<int> AddNewUserInCustomersInNotificationTopics(int userId, int groupId);
        Task<string> GetTopicName(int id);
    }
}
