using System.Text;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.UserNotificationConnection;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Notification;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.Notification.Commands;

namespace Ontec.Core.Application.Notification.Commands
{
    public class NotificationsCommandHandler : IRequestHandler<AddOrUpdateNotificationsQuery, AddUpdateResultDto>
                                                , IRequestHandler<UpdateUserConnectionIdQuery, int>
                                               , IRequestHandler<DeleteNotificationByIdQuery, string>
                                                , IRequestHandler<AddMasterNotifications, AddUpdateResultDto>
                                                , IRequestHandler<AddEditGroupQuery, AddUpdateResultDto>
                                                , IRequestHandler<AddCustomersInNotificationGroupsQuery, NotificationGroupResponseModel>
                                                , IRequestHandler<SendGroupNotificationRequest, NotificationGroupResponseModel>
                                                 , IRequestHandler<SubscribeTopicsforUsersRequestQuery, NotificationGroupResponseModel>
                                                , IRequestHandler<UnsubscribeQueryRequest, TopicManagementResponse>
    {
        private readonly IWorkContext _workContext;
        private readonly IUserRepository _userRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserNotificationConnection _userNotificationConnection;
        private readonly IMeterRepository _meterRepository;
        private readonly IPushNotification _pushNotification;
        private IHostingEnvironment _environment;
        private readonly IAuditTrail _auditTrail;
        public NotificationsCommandHandler(IWorkContext workContext
                                           , INotificationRepository notificationRepository
                                           , IUserRepository userRepository
                                           , IDocumentRepository dcoumentRepository
                                           , IPropertyRepository propertyRepository
                                           , IUserNotificationConnection userNotificationConnection
                                           , IMeterRepository meterRepository
                                            , IHostingEnvironment environment
                                            , IPushNotification pushNotification,
IAuditTrail auditTrail)
        {
            _workContext = workContext;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _userNotificationConnection = userNotificationConnection;
            _meterRepository = meterRepository;
            _propertyRepository = propertyRepository;
            _environment = environment;
            _pushNotification = pushNotification;
            _auditTrail = auditTrail;
        }
        public async Task<AddUpdateResultDto> Handle(AddOrUpdateNotificationsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new AddOrUpdateNotificationsQueryValidator(_workContext, _notificationRepository, _userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;

            result = await _notificationRepository.AddNotifications(request).ConfigureAwait(false);
            
            if (result > 0)
            {
                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.ModifiedBy = _workContext.CurrentUserId;
                    objAudit.Action = "Add Notification";
                    objAudit.ActionTable = "ohd_user_notifications";
                    objAudit.ModuleName = "Notification";
                    objAudit.StatusId = (int)StatusEnum.Sent;
                    objAudit.UpdatedId = result;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Notifications Saved successfully!";

            }
            return response;
        }

        public async Task<string> Handle(DeleteNotificationByIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new DeleteNotificationByIdQueryValidator(_notificationRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            await _notificationRepository.DeleteNotificationById(request.Ids).ConfigureAwait(false);
           
            return "Deleted successfully!";
        }

        public async Task<int> Handle(UpdateUserConnectionIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            return await _userNotificationConnection.UpdateUserNotificatioConnection(request).ConfigureAwait(false);
        }
        public async Task<AddUpdateResultDto> Handle(AddMasterNotifications request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new AddMasterNotificationValidator(_meterRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            int result = 0;

            int propertyId = await _meterRepository.GetPropertyIdByMeterNumber(request.MeterNumber).ConfigureAwait(false);
            var property = await _propertyRepository.GetPropertyById(propertyId).ConfigureAwait(false);
            int notificationId = await _notificationRepository.GetNotificationId(request.MeterNumber, request.NotificationType);

            var addnotification = new AddOrUpdateNotificationsQuery()
            {
                UserID = property.OwnerId,
                Title = request.Title,
                Description = request.Description,
                NotificationType = request.NotificationType,
                NoteStatus = request.NoteStatus,
                MeterNumber = request.MeterNumber
            };
            if (notificationId > 0)
            {
                result = await _notificationRepository.UpdateNotifications(notificationId, request.MeterNumber, request.NoteStatus.ToLower()).ConfigureAwait(false);
                response.Message = "Notification updated successfully";
            }
            else
            {
                result = await _notificationRepository.AddNotifications(addnotification).ConfigureAwait(false);
                response.Message = "Notification added successfully";
            }

            if (result > 0)
            {
                response.Id = result;
            }
            else
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(PayFastModel.m_payment_id),
                    ErrorMessage = "something went wrong"
                });
            }
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            return response;

        }
        public async Task<AddUpdateResultDto> Handle(AddEditGroupQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
           var objAudit = new AuditHelper();
            var commonValidator = new AddEditGroupQueryValidator(_notificationRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result = 0;
            if (request.Id == 0)
            {
                result = await _notificationRepository.AddNotificationGroup(request).ConfigureAwait(false);
                response.Message = "Group added successfully!";
                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.AddedBy = _workContext.CurrentUserId;
                    objAudit.Action = "Add Notification Group";
                    objAudit.ActionTable = "ohd_notificationgroups";
                    objAudit.ModuleName = "Notification Group";
                    objAudit.StatusId = (int)StatusEnum.Active;
                    objAudit.UpdatedId = result;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
            }
            if (request.Id > 0)
            {
                result = await _notificationRepository.UpdateNotificationGroup(request).ConfigureAwait(false);
                response.Message = "Group updated successfully!";

                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.AddedBy = _workContext.CurrentUserId;
                    objAudit.Action = "Delete Notification Group";
                    objAudit.ActionTable = "ohd_notificationgroups";
                    objAudit.ModuleName = "Notification Group";
                    objAudit.StatusId = (int)StatusEnum.Inactive;
                    objAudit.UpdatedId = result;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
            }
            if (result > 0)
            {
                response.Id = result;


            }
            return response;
        }



        public async Task<NotificationGroupResponseModel> Handle(AddCustomersInNotificationGroupsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new AddCustomersInNotificationGroupsQueryValidator(_notificationRepository, _userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new NotificationGroupResponseModel();

            var deviceTokens = new List<string>();
            foreach (var userId in request.UserIds)
            {
                deviceTokens.Add(await _userRepository.GetDeviceToken(userId).ConfigureAwait(false));

            }

            var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";


            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(jsonPath),

                });
            }

            //var credential = GoogleCredential.FromFile(jsonPath)
            //                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            using var client = new HttpClient();
            //if (credential.UnderlyingCredential is ServiceAccountCredential serviceAccountCredential)
            //{
            //    string accessToken = await serviceAccountCredential.GetAccessTokenForRequestAsync();

            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            //}

            client.DefaultRequestHeaders.TryAddWithoutValidation("project_id", "302520803240");



            string fcm_endpoint = "https://fcm.googleapis.com/fcm/notification";


            var groups = await _notificationRepository.GetGroups().ConfigureAwait(false);
            var groupName = groups
                            .Where(g => g.Id == request.GroupId)
                            .Select(g => g.Name)
                            .FirstOrDefault();


            var payload = new
            {
                operation = "create",
                notification_key_name = groupName,
                registration_ids = deviceTokens
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var fcmResponse = await client.PostAsync(fcm_endpoint, content);
            var responseContent = await fcmResponse.Content.ReadAsStringAsync();

            //using var client = new HttpClient();

            //var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";
            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    var credential = GoogleCredential.FromFile(jsonPath)
            //        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            //    if (credential.UnderlyingCredential is ServiceAccountCredential serviceAccountCredential)
            //    {
            //        string accessToken = await serviceAccountCredential.GetAccessTokenForRequestAsync();
            //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //        client.DefaultRequestHeaders.TryAddWithoutValidation("project_id", "302520803240");
            //    }
            //    FirebaseApp.Create(new AppOptions()
            //    {
            //        Credential = credential
            //    });
            //}
            //else
            //{
            //    var credential = GoogleCredential.FromFile(jsonPath)
            //.CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            //    if (credential.UnderlyingCredential is ServiceAccountCredential serviceAccountCredential)
            //    {
            //        string accessToken = await serviceAccountCredential.GetAccessTokenForRequestAsync();
            //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //        client.DefaultRequestHeaders.TryAddWithoutValidation("project_id", "302520803240");
            //    }
            //}

            //var groups = await _notificationRepository.GetGroups().ConfigureAwait(false);
            //var groupName = groups
            //    .Where(g => g.Id == request.GroupId)
            //    .Select(g => g.Name)
            //    .FirstOrDefault();

            //var payload = new
            //{
            //    operation = "create",
            //    notification_key_name = groupName,
            //    registration_ids = deviceTokens // Ensure deviceTokens is not null/empty
            //};

            //var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            //var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            //string fcm_endpoint = "https://fcm.googleapis.com/fcm/notification";

            //var fcmResponse = await client.PostAsync(fcm_endpoint, content);
            //var responseContent = await fcmResponse.Content.ReadAsStringAsync();


            if (deviceTokens != null)
            {
                if (deviceTokens.Count() > 0)
                {
                    response.Id = await _notificationRepository.AddCustomersInNotificationGroups(request, responseContent).ConfigureAwait(false);
                }
                if (response.Id > 0)
                {
                    response.Message = "Notification Group Created successfully!";
                    response.NotificationKey = responseContent;
                }
            }


            return response;
        }


        public async Task<string> Handle(SendGroupNotifications request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var response = "";
            var commonValidator = new SendGroupNotificationsValidator(_notificationRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            string topic = "";
            var group = await _notificationRepository.GetGroups().ConfigureAwait(false);
            topic = group.FirstOrDefault(g => g.Id == request.GroupId)?.OtherText;
            if (!string.IsNullOrEmpty(topic))
            {
                response = await _pushNotification.SendMessageToGroupAsync(topic, request.Title, request.Body).ConfigureAwait(false);

                AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                {
                    UserID = _workContext.CurrentUserId,
                    Title = request.Title,
                    Description = request.Body,
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.firebase,

                };
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            }
            return response;
        }
        public async Task<NotificationGroupResponseModel> Handle(SubscribeTopicsforUsersRequestQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new SubscribeTopicsforUsersRequestQueryValidator(_notificationRepository, _userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new NotificationGroupResponseModel();

            //var deviceTokens = new List<string>();
            //foreach (var userId in request.UserIds)
            //{
            //    deviceTokens.Add(await _userRepository.GetDeviceToken(userId).ConfigureAwait(false));

            //}
            //var group = await _notificationRepository.GetGroups().ConfigureAwait(false);
            //var groupName = group.FirstOrDefault(g => g.Id == request.GroupId)?.OtherText;

            //var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";

            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    FirebaseApp.Create(new AppOptions()
            //    {
            //        Credential = GoogleCredential.FromFile(jsonPath),
            //    });
            //}
            try
            {


                //if (deviceTokens != null)
                //{
                //    deviceTokens = deviceTokens
                //                .Where(token => !string.IsNullOrWhiteSpace(token))
                //                .ToList();

                    //if (deviceTokens.Count() > 0)
                    //{
                       // var topicResponse = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(deviceTokens, groupName);
                        response.Id = await _notificationRepository.AddCustomersInNotificationTopics(request).ConfigureAwait(false);
                    //}
                    if (response.Id > 0)
                    {
                        response.Message = "Notification Group Created successfully!";
                    }
                    else
                    {
                        response.Message = "Device tokens not found users not added in group!";
                    }
                //}

            }
            catch (FirebaseMessagingException ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<TopicManagementResponse> Handle(UnsubscribeQueryRequest request, CancellationToken cancellationToken)
        {
            var tokens = await _userRepository.GetDeviceTokens().ConfigureAwait(false);
            var staleTokens = tokens
                              .Where(t =>
                                  t.LastActive < DateTime.UtcNow.AddDays(-30) ||
                                  t.CreatedAt < DateTime.UtcNow.AddDays(-30))
                              .Select(t => t.Token)
                              .ToList();
            var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(jsonPath),
                });
            }
            var messaging = FirebaseMessaging.DefaultInstance;
            var response = await messaging.UnsubscribeFromTopicAsync(staleTokens, request.Topic);
            return response;

        }
        public async Task<NotificationGroupResponseModel> Handle(SendGroupNotificationRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new SendGroupNotificationRequestValidator(_workContext, _notificationRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new NotificationGroupResponseModel();
            List<int> propertyIds = new List<int>();
            var deviceTokens = new List<string>();
            var topic = "";
            var group = await _notificationRepository.GetGroups().ConfigureAwait(false);
            var groupName = group.FirstOrDefault(g => g.Id == request.GroupId)?.OtherText;
            if (!string.IsNullOrEmpty(groupName))
            {
               topic = groupName.Replace(" ", "-");
            }
           var activeUserIds= new List<int>();
           
             
            activeUserIds= (List<int>)await _userRepository.GetActiveUserIds(request.UserIds).ConfigureAwait(false);
            //get devicetoken for users in that group
            if (activeUserIds.Any())
            {
                foreach (var id in activeUserIds)
                {
                    deviceTokens.Add(await _userRepository.GetDeviceToken(id).ConfigureAwait(false));

                }
            }
            var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(jsonPath),
                });
            }

            //add devicetokens in topic
            try
            {
                if (deviceTokens != null)
                {
                    deviceTokens = deviceTokens
                                .Where(token => !string.IsNullOrWhiteSpace(token))
                                .ToList();

                    if (deviceTokens.Count() > 0)
                    {
                        var topicResponse = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(deviceTokens, topic);
                    }
                }
            }
            catch (Exception)
            { }


            //send notifications to topics
            if (!string.IsNullOrEmpty(topic))
            {
                response.Message = await _pushNotification.SendMessageToGroupAsync(topic, request.Title, request.Body).ConfigureAwait(false);

                foreach (var u in activeUserIds)
                {
                    AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                    {
                        UserID = u,
                        Title = request.Title,
                        Description = request.Body,
                        IsRead = (int)StatusEnum.Sent,
                        NotificationType = (int)NotificationType.firebase,

                    };
                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                }
            }
            return response;
        }
    }
}
