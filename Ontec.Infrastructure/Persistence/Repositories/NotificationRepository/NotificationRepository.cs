using Dapper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Notification;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.Notification.Commands;
using Ontec.Core.Domain.Requests.Notification.Queries;

namespace Ontec.Infrastructure.Persistence.Repositories.NotificationRepository
{
    public class NotificationRepository(IGenericRepository genericRepository, IWorkContext workContext, IAuditTrail auditTrail,IUserRepository userRepository) : INotificationRepository
    {
        private readonly IGenericRepository _genericRepository = genericRepository;
        private readonly IUserRepository _userRepository=userRepository;
        private readonly IWorkContext _workContext = workContext;
        private readonly IAuditTrail _auditTrail=auditTrail;
        public async Task<IEnumerable<OntecSelectListItem>> GetNotificationTypeMasters()
        {
            var sQuery = @"SELECT Id,notification_type as name
	                       FROM public.ohd_notification_type_master 
	                       WHERE status_id=@Status;";
            var parameters = new DynamicParameters();
            parameters.Add("@Status", (int)StatusEnum.Active);

            return await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> GetNotificationId(string meterNumber, int notificationTypeId)
        {
            Note_status status = Note_status.Open;
            string noteStatus = status.ToString().ToLower();
            var sQuery = @"SELECT Id
                            FROM public.ohd_user_notifications 
	                       WHERE note_status=@NoteStatus AND meter_number=@MeterNumber AND notification_type=@NotificationType;";
            var parameters = new DynamicParameters();
            parameters.Add("@NoteStatus", noteStatus);
            parameters.Add("@MeterNumber", meterNumber);
            parameters.Add("@NotificationType", notificationTypeId);

            int id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return id;
        }
        public async Task<int> AddNotifications(AddOrUpdateNotificationsQuery request)
        {
            var sQuery = @" INSERT INTO ohd_user_notifications(
                            user_id,
                            title,
                            description,
                            notification_type,
                            status_id, 
                            note_status,
                            meter_number,
                            created_at)VALUES
                            (@UserId,                               
                             @Title,
                            @Decscription,
                            @NotificationType,
                            @statusId,
                            @NoteStatus,
                            @MeterNumber,
                            @CreatedAt)
                            RETURNING lastval()";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", request.UserID);
            parameters.Add("@Title", request.Title);
            parameters.Add("@Decscription", request.Description);
            parameters.Add("@NotificationType", request.NotificationType);
            parameters.Add("@statusId", (int)StatusEnum.Sent);
            parameters.Add("@MeterNumber", request.MeterNumber);
            if (request.NoteStatus != null)
            {
                parameters.Add("@NoteStatus", request.NoteStatus.ToLower());
            }
            else
            {
                parameters.Add("@NoteStatus", null);
            }
            if (request.MeterNumber != null)
            {
                parameters.Add("@MeterNumber", request.MeterNumber);
            }
            else
            {
                parameters.Add("@MeterNumber", null);
            }
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> UpdateNotifications(int notificationId, string meterNumber, string noteStatus)
        {
            var sQuery = @"UPDATE ohd_user_notifications SET 
                         status_id=@statusId ,note_status=@NoteStatus
                         WHERE 
                          note_status=@OpenNoteStatus
                         AND meter_number=@MeterNumber
                         AND id =@NotificationId;
                         SELECT id FROM ohd_user_notifications 
                         WHERE id=@NotificationId;";
            var parameters = new DynamicParameters();
            parameters.Add("@OpenNoteStatus", "open");
            parameters.Add("@NoteStatus", noteStatus);
            parameters.Add("@MeterNumber", meterNumber);
            parameters.Add("NotificationId", notificationId);
            parameters.Add("@statusId", (int)StatusEnum.Read);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<bool> IsNotificationTypeExist(int id)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_notification_type_master  
                          WHERE id=@Id  ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result > 0;
        }
        public async Task<int> IsNotificationsExist(int id)
        {
            var sQuery = @"SELECT id
                            FROM ohd_user_notifications  
                          WHERE id=@Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<NotificationsDto> GetNotificationsByUserId(GetNotificationsByUserIdQuery request)
        {
            NotificationsDto notificationsDto = new();
            var sQuery = @"SELECT  id
                            ,user_id As UserId
                            ,title as Title
                            ,description As Description
                            ,notification_type AS NotificationType
                            ,CASE WHEN status_id = @Read THEN 1 ELSE 0 END as IsRead
                           ,created_at as CreatedAt
                            --,  TO_CHAR(created_at::date, 'dd-MM-yyyy') as CreatedAt
                            ,modified_at AS ModifiedAt
                            ,note_status As NoteStatus
                           FROM public.ohd_user_notifications
                           WHERE user_id=@UserId  and status_id!=@InActive";


            var sCountQuery = @"SELECT COUNT(id) as Count from public.ohd_user_notifications
                                WHERE user_id=@UserId AND status_id!=@InActive";
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", request.UserId);
            parameters.Add("@Read", (int)StatusEnum.Read);
            parameters.Add("@InActive", (int)StatusEnum.Inactive);
            if (request.NotificationTypeId > 0)
            {
                sQuery += " AND notification_type =@NotificationType;";
                //parameters.Add("@NotificationType", (int)NotificationType.Register);
                parameters.Add("@NotificationType", request.NotificationTypeId);
            }
            if (!string.IsNullOrEmpty(request.NoteStatus) && request.NoteStatus != "string")
            {
                sQuery += " AND note_status=@NoteStatus;";
                parameters.Add("@NoteStatus", request.NoteStatus.ToLower());
            }

            var notificationsList = _genericRepository.GetAsync<UserNotification>(sQuery, parameters);
            var totalCount = _genericRepository.ExecuteScalarAsync<int>(sCountQuery, parameters);
            try
            {
                await Task.WhenAll(notificationsList, totalCount).ConfigureAwait(false);
            }
            catch (Exception ex) { }

            notificationsDto.Notification = notificationsList.Result.ToList();
            notificationsDto.Count = totalCount.Result;

            var notifications = notificationsDto.Notification.ToList();

            notifications = [.. notifications.OrderByDescending(t => t.CreatedAt)];
            if (request.IsShortView)
            {
                notifications = notifications.Take(5).ToList();
            }

            var transformedNotifications = notifications
                .Select(a => new UserNotification
                {
                    Id = a.Id,
                    IsRead = a.IsRead,
                    UserId = a.UserId,
                    Title = a.Title,
                    Description = a.Description,
                    NotificationType = a.NotificationType,
                    CreatedAt = a.CreatedAt,
                    ModifiedAt = a.ModifiedAt,
                    TimeAgo = GetTimeAgo(a.CreatedAt),
                    NoteStatus = a.NoteStatus
                });

            notificationsDto.Notification = transformedNotifications.ToList();
            notificationsDto.Count = totalCount.Result;

            return notificationsDto;
        }


        public async Task DeleteNotificationById(IEnumerable<int> ids)
        {
            var objAudit = new AuditHelper();
            var sQuery = @"UPDATE public.ohd_user_notifications
                         SET status_id=@StatusId, modified_at = @ModifiedAt 
                         WHERE id in (";
            var parameters = new DynamicParameters();
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            //int count = 0;
            //foreach (var id in ids)
            //{
            //    //sQuery += "@Id" + count.ToString() + ",";
            //    parameters.Add("@Id" + count.ToString(), id);
            //    if (count < count - 1)
            //    {
            //        sQuery += ",";
            //    }
            //    count++;
            //}
            //sQuery += " )";

            var idsList = ids.ToList();
            int count = 0;
            foreach (var id in idsList)
            {
                sQuery += "@Id" + count.ToString();

                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.ModifiedBy = _workContext.CurrentUserId;
                    objAudit.Action = "Delete Notification";
                    objAudit.ActionTable = "ohd_user_notifications";
                    objAudit.ModuleName = "Notification";
                    objAudit.StatusId = (int)StatusEnum.Inactive;
                    objAudit.UpdatedId =id;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
                if (count < idsList.Count - 1)
                {
                    sQuery += ",";
                }

                parameters.Add("@Id" + count.ToString(), id);


                count++;
            }
            sQuery += " )";

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        private static string GetTimeAgo(DateTime creationDate)
        {
            var currentTime = DateTime.UtcNow;
            creationDate = creationDate.ToUniversalTime();
            TimeSpan difference = currentTime - creationDate;

            int hours = (int)difference.TotalHours;
            if (hours < 24)
            {
                int minutes = (int)difference.TotalMinutes % 60;
                if (hours == 0)
                {
                    int seconds = (int)difference.TotalSeconds % 60;
                    return $"{minutes}m {seconds}s ago";
                }
                else
                    return $"{hours}h {minutes}m ago";
            }
            else
            {
                return creationDate.ToString("dd-MM-yyyy HH:MM tt");
            }
        }

        public async Task<int> AddNotificationGroup(AddEditGroupQuery request)
        {
            var sQuery = @"INSERT INTO public.ohd_notificationgroups(
	                            group_name, 
                                created_at, 
                                status_id,estate_id,topic_name)
	                    VALUES (@Group,
                                @CreatedAt,
                                @StatusId,@EstateId,@TopicName)RETURNING lastval();";

            var parameters = new DynamicParameters();
            parameters.Add("@Group", request.GroupName.ToLower());
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            parameters.Add("@statusId", (int)StatusEnum.Active);
            parameters.Add("@TopicName", request.GroupName.Replace(" ", ""));
            if (request.EstateId.HasValue)
            {
                parameters.Add("@EstateId", request.EstateId);
            }
            else
            {
                parameters.Add("@EstateId", null);
            }
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> UpdateNotificationGroup(AddEditGroupQuery request)
        {
            var sQuery = @"UPDATE public.ohd_notificationgroups
	                         SET 
                                status_id= @StatusId
                            WHERE id=@Id;
                            SELECT id FROM  public.ohd_notificationgroups
                            HERE id=@Id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Group", request.GroupName.ToLower());
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            parameters.Add("@statusId", (int)StatusEnum.Inactive);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> ActiveNotificationGroup(AddEditGroupQuery request)
        {
            var sQuery = @"UPDATE public.ohd_notificationgroups
	                         SET 
                                status_id= @StatusId
                            WHERE id=@Id;
                            SELECT id FROM  public.ohd_notificationgroups
                            HERE id=@Id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Group", request.GroupName.ToLower());
            parameters.Add("@statusId", (int)StatusEnum.Active);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<IEnumerable<OntecSelectListItem>> GetGroups()
        {
            var sQuery = @"SELECT Id,group_name as name,topic_name as OtherText
	                       FROM public.ohd_notificationgroups 
	                       WHERE status_id=@Status;";
            var parameters = new DynamicParameters();
            parameters.Add("@Status", (int)StatusEnum.Active);

            return await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<IEnumerable<OntecSelectListItem>> GetUsers()
        {
            var sQuery = @"select id As Id, CONCAT(first_name,' ',last_name) AS name FROM public.ohd_user 
                            WHERE status_id=@Status AND role_id=@RoleId;";
            var parameters = new DynamicParameters();
            parameters.Add("@Status", (int)StatusEnum.Active);
            parameters.Add("@RoleId", (int)RoleMasterEnum.Customer);
            try
            {
                return await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            { throw ex; }
        }
        public async Task<int> IsGroupIdExist(int id)
        {
            var sQuery = @"SELECT Id
	                       FROM public.ohd_notificationgroups 
	                       WHERE id=@Id AND status_id=@Active;";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@Active", (int)StatusEnum.Active);

            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<int> IsGroupExist(string group)
        {
            var sQuery = @"SELECT Id
	                       FROM public.ohd_notificationgroups 
	                       WHERE group_name=@Group AND status_id=@Active";
            var parameters = new DynamicParameters();
            parameters.Add("@Group", group);
            parameters.Add("@Active", (int)StatusEnum.Active);

            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> IsInActiveGroupExist(string group)
        {
            var sQuery = @"SELECT Id
	                       FROM public.ohd_notificationgroups 
	                       WHERE group_name=@Group AND Status_id=@InActive;";
            var parameters = new DynamicParameters();
            parameters.Add("@Group", group);
            parameters.Add("@InActive", (int)StatusEnum.Inactive);

            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<int> AddCustomersInNotificationGroups(AddCustomersInNotificationGroupsQuery request, string notificationKey)
        {
            int id = 0;
            var vr = new GetGroupWiseUsersQuery()
            {
                //GroupId = request.GroupId,
            };
            bool isContained = false;
            List<int> existUserIds = new List<int>();

            var existingUsers = await GetGroupWiseUsers(vr).ConfigureAwait(false);
            if (existingUsers == null || !existingUsers.Any())
            {
                id = await InsertGroupUsers(request.GroupId, request.UserIds, notificationKey).ConfigureAwait(false);
            }
            else
            {

                if (existingUsers != null && existingUsers.Count() >= 1)
                {
                    int status = (int)StatusEnum.Active;
                    existUserIds = existingUsers.Where(t => t.StatusId.Equals(status.ToString()))
                                                              .Select(t => t.UserId).ToList();
                    isContained = request.UserIds.Any(id => existUserIds.Contains(id));

                }
                if (existingUsers != null && isContained)
                {

                    await UpdateGroupUsers(request.UserIds, request.GroupId, existUserIds).ConfigureAwait(false);
                    id = await InsertGroupUsers(request.GroupId, request.UserIds, notificationKey).ConfigureAwait(false);
                }
                else
                {
                    await UpdateGroupUsers(request.UserIds, request.GroupId, existUserIds).ConfigureAwait(false);
                    id = await InsertGroupUsers(request.GroupId, request.UserIds, notificationKey).ConfigureAwait(false);
                }

            }

            return id;
        }
        public async Task<int> AddCustomersInNotificationTopics(SubscribeTopicsforUsersRequestQuery request)
        {
            var userIds = new List<int>();
            int id = 0;
            var vr = new GetGroupWiseUsersQuery()
            {
                //GroupId = request.GroupId,
            };
            bool isContained = false;
            List<int> existUserIds = new List<int>();

            var existingUsers = await GetGroupWiseUsersByGroupId(request.GroupId).ConfigureAwait(false);
            if ((existingUsers == null || !existingUsers.Any()))
            {
                id = await InsertTopicUsers(request.GroupId, request.UserIds).ConfigureAwait(false);
            }
            if (existingUsers != null && existingUsers.Any(u => u.StatusId == null))
            {
                // If ANY user has StatusId != null → insert those
                
                    id = await InsertTopicUsers(request.GroupId,request.UserIds).ConfigureAwait(false);
                
            }
            else
            {
                if (existingUsers != null && existingUsers.Count() >= 1)
                {
                    int status = (int)StatusEnum.Active;
                    existUserIds = existingUsers.Where(t => t.StatusId.Equals(status.ToString()))
                                                              .Select(t => t.UserId).ToList();
                    isContained = request.UserIds.Any(id => existUserIds.Contains(id));

                }
                if (existingUsers != null && isContained)
                {
                    await UpdateGroupUsers(request.UserIds, request.GroupId, existUserIds).ConfigureAwait(false);
                    foreach (int userId in request.UserIds)
                    {
                        userIds.Add(userId);
                    }
                    foreach (int userId in existUserIds)
                    {
                        userIds.Add(userId);
                    }
                    id = await InsertTopicUsers(request.GroupId, request.UserIds).ConfigureAwait(false);

                }
                else
                {
                    await UpdateGroupUsers(request.UserIds, request.GroupId, existUserIds).ConfigureAwait(false);
                    foreach (int userId in request.UserIds)
                    {
                        userIds.Add(userId);
                    }
                    foreach (int userId in existUserIds)
                    {
                        userIds.Add(userId);
                    }
                    id = await InsertTopicUsers(request.GroupId, userIds).ConfigureAwait(false);
                }

            }

            return id;
        }


        public async Task<int> InsertGroupUsers(int groupId, IEnumerable<int> UserIds, string notificationKey)
        {
            int id = 0;
            foreach (var userId in UserIds)
            {
                var sQuery = @"INSERT INTO public.ohd_notification_custome_group_linking(
	                                     group_id,
                                         customer_id,
                                         created_at,
                                        status_id,
                                        notificationkey)
	                            VALUES (@GroupId,
                                        @CustomerId,
                                        @CreatedAt,
                                        @StatusId,
                                        @NotificationKey)
                                RETURNING lastval();";

                var parameters = new DynamicParameters();
                parameters.Add("@GroupId", groupId);
                parameters.Add("@CreatedAt", DateTime.UtcNow);
                parameters.Add("@CustomerId", userId);
                parameters.Add("@StatusId", (int)StatusEnum.Active);
                parameters.Add("@NotificationKey", notificationKey);
                try
                {
                    id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return id;
                }

            }
            return id;
        }
        public async Task<int> InsertTopicUsers(int groupId, IEnumerable<int> UserIds)
        {
            int id = 0;
            foreach (var userId in UserIds)
            {
                if (userId > 0)
                {
                    var user = await _userRepository.GetUserById(userId).ConfigureAwait(false);
                    if (user.StatusId != (int)StatusEnum.Inactive)
                    {

                        var sQuery = @"INSERT INTO public.ohd_notification_custome_group_linking(
	                                     group_id,
                                         customer_id,
                                         created_at,
                                        status_id
                                        )
	                            VALUES (@GroupId,
                                        @CustomerId,
                                        @CreatedAt,
                                        @StatusId
                                        )
                                RETURNING lastval();";

                        var parameters = new DynamicParameters();
                        parameters.Add("@GroupId", groupId);
                        parameters.Add("@CreatedAt", DateTime.UtcNow);
                        parameters.Add("@CustomerId", userId);
                        parameters.Add("@StatusId", (int)StatusEnum.Active);
                        try
                        {
                            id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            return id;
                        }
                    }
                }
            }
            return id;
        }

        public async Task<int> InsertTopicUser(int groupId, int userId)
        {
            int id = 0;

            var sQuery = @"INSERT INTO public.ohd_notification_custome_group_linking(
	                                     group_id,
                                         customer_id,
                                         created_at,
                                        status_id
                                        )
	                            VALUES (@GroupId,
                                        @CustomerId,
                                        @CreatedAt,
                                        @StatusId
                                        )
                                RETURNING lastval();";

            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", groupId);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            parameters.Add("@CustomerId", userId);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            try
            {
                id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return id;
            }


            return id;
        }
        public async Task UpdateGroupUsers(IEnumerable<int> userId, int groupId, IEnumerable<int> existUserIds)
        {
            try
            {
                var parameters = new DynamicParameters();
                if (existUserIds.Count() > 0)
                {
                    foreach (var existuserId in existUserIds)
                    {
                        var sQuery = @"UPDATE public.ohd_notification_custome_group_linking
                           SET status_id=@StatusId 
                               ,modified_at=@ModifiedAt
                           WHERE group_id=@GroupId AND customer_id=@UserId;";
                        parameters.Add("@UserId", existuserId);
                        parameters.Add("@StatusId", (Int32)StatusEnum.Inactive);
                        parameters.Add("@ModifiedAt", DateTime.UtcNow);
                        parameters.Add("@GroupId", groupId);
                        await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<GroupWiseUsersDto>> GetGroupWiseUsers(GetGroupWiseUsersQuery request)
        {

            //var sQuery = @"SELECT ts.id as Id,
            //            ts.group_id as GroupId,
            //            sm.group_name as GroupName,
            //            CONCAT(u.first_name,' ',u.last_name)AS Customer,
            //            customer_id as UserId,
            //            ts.status_id as StatusId,
            //            ts.notificationkey As NotificationKey,
            //             ud.device_token AS DeviceToken
            //            FROM public.ohd_notification_custome_group_linking as ts
            //            LEFT JOIN public.ohd_notificationgroups as sm ON ts.group_id=sm.id
            //            LEFT JOIN public.ohd_user as u on ts.customer_id=u.id
            //            LEFT JOIN public.ohd_user_device_token as ud on ts.customer_id=ud.user_id
            //            WHERE u.status_id=@Active AND  ts.status_id=@Active and sm.status_id=@Active";

            var sQuery = @"SELECT 
                            ng.id AS GroupId,
                            ng.group_name AS GroupName,
                            ngl.customer_id AS UserId,
                            CONCAT(COALESCE(u.first_name, ''), ' ', COALESCE(u.last_name, '')) AS Customer,
                            ud.device_token AS DeviceToken  ,
                            ng.status_id AS StatusId
                        FROM public.ohd_notificationgroups AS ng
                        LEFT JOIN public.ohd_notification_custome_group_linking AS ngl ON ng.id = ngl.group_id AND ngl.status_id = @Active
                        LEFT JOIN public.ohd_user AS u ON ngl.customer_id = u.id
                        LEFT JOIN public.ohd_user_device_token AS ud ON u.id = ud.user_id
                         WHERE  ng.status_id=@Active";

            var parameter = new DynamicParameters();
            //parameter.Add("@GroupId", request.GroupId);
            parameter.Add("@Active", (int)StatusEnum.Active);
            return await _genericRepository.GetAsync<GroupWiseUsersDto>(sQuery, parameter);
        }

        public async Task<IEnumerable<GroupWiseUsersDto>> GetGroupWiseUsersByGroupId(int id)
        {

            //var sQuery = @"SELECT ts.id as Id,
            //            ts.group_id as GroupId,
            //            sm.group_name as GroupName,
            //            CONCAT(u.first_name,' ',u.last_name)AS Customer,
            //            customer_id as UserId,
            //            ts.status_id as StatusId,
            //            ts.notificationkey As NotificationKey,
            //             ud.device_token AS DeviceToken
            //            FROM public.ohd_notification_custome_group_linking as ts
            //            LEFT JOIN public.ohd_notificationgroups as sm ON ts.group_id=sm.id
            //            LEFT JOIN public.ohd_user as u on ts.customer_id=u.id
            //            LEFT JOIN public.ohd_user_device_token as ud on ts.customer_id=ud.user_id
            //            WHERE u.status_id=@Active AND  ts.status_id=@Active and sm.status_id=@Active";

            var sQuery = @"SELECT 
                            ng.id AS GroupId,
                            ng.group_name AS GroupName,
                            ngl.customer_id AS UserId,
                            CONCAT(COALESCE(u.first_name, ''), ' ', COALESCE(u.last_name, '')) AS Customer,
                            ud.device_token AS DeviceToken  ,
                            ngl.status_id AS StatusId
                        FROM public.ohd_notificationgroups AS ng
                        LEFT JOIN public.ohd_notification_custome_group_linking AS ngl ON ng.id = ngl.group_id AND ngl.status_id = @Active
                        LEFT JOIN public.ohd_user AS u ON ngl.customer_id = u.id
                        LEFT JOIN public.ohd_user_device_token AS ud ON u.id = ud.user_id
                         WHERE  ng.status_id=@Active AND ng.id=@Id";

            var parameter = new DynamicParameters();
            //parameter.Add("@GroupId", request.GroupId);
            parameter.Add("@Active", (int)StatusEnum.Active);
            parameter.Add("@Id", id);
            return await _genericRepository.GetAsync<GroupWiseUsersDto>(sQuery, parameter);
        }

        public async Task<int> IsNotifiationKeyExist(string key)
        {
            var sQuery = @"SELECT count(id)
	                       FROM public.ohd_notification_custome_group_linking 
	                       WHERE notificationkey=@NotificationKey;";
            var parameters = new DynamicParameters();
            parameters.Add("@NotificationKey", key);

            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> GetGroupIdByEstateId(int estateId)
        {
            var sQuery = @"SELECT Id
	                       FROM public.ohd_notificationgroups 
	                       WHERE estate_id=@EstateId;";
            var parameters = new DynamicParameters();
            parameters.Add("@EstateId", estateId);

            return await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task DeleteUserFromNotifications(int userId)
        {
            var sQuery = @"DELETE 
                         FROM public.ohd_notification_custome_group_linking  
                         WHERE customer_id=@UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> AddNewUserInCustomersInNotificationTopics(int userId, int groupId)
        {

            var userIds = new List<int>();
            int id = 0;
            var vr = new GetGroupWiseUsersQuery()
            {
                //GroupId = request.GroupId,
            };
            bool isContained = false;
            List<int> existUserIds = new List<int>();

            var existingUsers = await GetGroupWiseUsersByGroupId(groupId).ConfigureAwait(false);

            if (existingUsers != null && existingUsers.Count() >= 1 && existingUsers.Any(u => u.UserId != null && u.StatusId != null))
            {

                int status = (int)StatusEnum.Active;
                existUserIds = existingUsers.Where(t => t.StatusId.Equals(status.ToString()))
                                                          .Select(t => t.UserId).ToList();
                isContained = userIds.Any(id => existUserIds.Contains(id));

            }
            if (existingUsers != null && !isContained && existingUsers.Any(u => u.UserId != null && u.StatusId != null))
            {

                await UpdateGroupUsers(userIds, groupId, existUserIds).ConfigureAwait(false);
                foreach (var e in existUserIds)
                {
                    userIds.Add(e);
                }
                userIds.Add(userId);
                id = await InsertTopicUsers(groupId, userIds).ConfigureAwait(false);

            }
            else
            {
                userIds.Add(userId);
                id = await InsertTopicUsers(groupId, userIds).ConfigureAwait(false);
            }



            return id;
        }
        public async Task<string> GetTopicName(int id)
        {
            var sQuery = @"SELECT topic_name 
	                       FROM public.ohd_notificationgroups 
	                       WHERE id=@Id;";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await _genericRepository.ExecuteScalarAsync<string>(sQuery, parameters).ConfigureAwait(false);
        }
    }
}

