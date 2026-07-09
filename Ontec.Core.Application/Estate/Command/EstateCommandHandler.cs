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
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Estate;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Notification;
using Ontec.Core.Domain.Requests.Estate.Command;
using Ontec.Core.Domain.Requests.Notification.Command;
using static Ontec.Core.Domain.Models.Dto.STSPurchase.STSPurchasesDto;

namespace Ontec.Core.Application.Estate.Command
{
    public class EstateCommandHandler : IRequestHandler<AddEstateRequestCommand, AddUpdateResultDto>,
                                        IRequestHandler<DeleteEstateRequestCommand, string>,
                                        IRequestHandler<SendEstateNotificationRequest, NotificationGroupResponseModel>

    {
        private readonly IEstateRepository _estateRepository;
        private readonly IWorkContext _workContext;
        private readonly ICompanyRepository _companyRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IPushNotification _pushNotification;
        private readonly IUserRepository _userRepository;
        private IHostingEnvironment _environment;
        private readonly IAuditTrail _auditTrail;
        public EstateCommandHandler(IEstateRepository estateRepository,
                                        IWorkContext workContext,
                                        INotificationRepository notificationRepository,
                                        ICompanyRepository companyRepository,
                                        IPushNotification pushNotification,
                                        IUserRepository userRepository,
                                        IHostingEnvironment environment,
                                        IAuditTrail auditTrail)
        {
            _estateRepository = estateRepository;
            _workContext = workContext;
            _notificationRepository = notificationRepository;
            _companyRepository = companyRepository;
            _pushNotification = pushNotification;
            _userRepository = userRepository;
            _environment = environment;
            _auditTrail = auditTrail;
        }
        public async Task<AddUpdateResultDto> Handle(AddEstateRequestCommand request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new AddEstateRequestCommandValidator(_estateRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            int topicId = 0;
            var response = new AddUpdateResultDto();
            int result;
            var company = await _companyRepository.GetCompanyDetails(_workContext.CurrentCompanyId).ConfigureAwait(false);
            if (request.Id > 0)
            {

                result = await _estateRepository.UpdateEstate(request).ConfigureAwait(false);
                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.AddedBy = _workContext.CurrentUserId;
                    objAudit.ActionTable = "ohd_estate";
                    objAudit.ModuleName = "Estate";

                    objAudit.StatusId = request.StatusId;
                    objAudit.Action = "Estate Updated";

                    objAudit.EntityName = request.Estate;
                    objAudit.UpdatedId = result;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
                //var topicReq = new AddEditGroupQuery()
                //{
                //    GroupName = request.Estate.Replace(" ", "-")
                //};
                // topicId = await _notificationRepository.AddNotificationGroup(topicReq).ConfigureAwait(false);
            }
            else
            {


                result = await _estateRepository.AddEstate(request).ConfigureAwait(false);
                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.AddedBy = _workContext.CurrentUserId;
                    objAudit.ActionTable = "ohd_estate";
                    objAudit.ModuleName = "Estate";

                    objAudit.StatusId = (int)StatusEnum.Active;
                    objAudit.Action = "Estate Added";

                    objAudit.EntityName = request.Estate;
                    objAudit.UpdatedId = result;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }

                //var topicReq = new AddEditGroupQuery()
                //{
                //    GroupName = request.Estate.Replace(" ", "-"),
                //    EstateId = result
                //};
                // topicId = await _notificationRepository.AddNotificationGroup(topicReq).ConfigureAwait(false);
                AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                {
                    UserID = _workContext.CurrentUserId,
                    Title = "New estate registered",
                    Description = request.Estate + " - " + "estate registered in " + company.CompanyName,
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Register

                };
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            }

            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Records created successfully!";


                else
                {
                    AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                    {
                        UserID = _workContext.CurrentUserId,
                        Title = "Estate updated",
                        Description = request.Estate + " - " + "estate updated in " + company.CompanyName,
                        IsRead = (int)StatusEnum.Sent,
                        NotificationType = (int)NotificationType.Updated

                    };
                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                    response.Message = "Records updated successfully!";
                }
            }
            return response;
        }

        public async Task<string> Handle(DeleteEstateRequestCommand request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new DeleteEstateRequestCommandValidator(_estateRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var estate = await _estateRepository.GetEstateById(request.Id).ConfigureAwait(false);
            var estateName = estate.Estate;

            await _estateRepository.DeleteEstate(request.Id).ConfigureAwait(false);

            objAudit.AddedBy = _workContext.CurrentUserId;
            objAudit.ActionTable = "ohd_estate";
            objAudit.ModuleName = "Estate";

            objAudit.StatusId = (int)StatusEnum.Inactive;
            objAudit.Action = "Estate deleted";

            objAudit.EntityName = estate.Estate;
            objAudit.UpdatedId = request.Id;
            await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);

            var company = await _companyRepository.GetCompanyDetails(_workContext.CurrentCompanyId).ConfigureAwait(false);
            AddOrUpdateNotificationsQuery newNotification = new()
            {
                UserID = _workContext.CurrentUserId,
                Title = "Estate deleted",
                Description = "Estate : " + estateName + " deleted from " + company.CompanyName,
                IsRead = (int)StatusEnum.Sent,
                NotificationType = (int)NotificationType.Deleted

            };
            await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

            return "Estate deleted successfully!";
        }



        public async Task<NotificationGroupResponseModel> Handle(SendEstateNotificationRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new SendEstateNotificationRequestValidator(_workContext, _estateRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new NotificationGroupResponseModel();
            List<int> propertyIds = new List<int>();
            var deviceTokens = new List<string>();
            if (!string.IsNullOrEmpty(request.Topic))
            {
                request.Topic = request.Topic.Replace(" ", "-");
            }
            //get all propertyIds under requested estate
            var properties = await _estateRepository.GetPropertiesByEstateId(request.EstateId).ConfigureAwait(false);
            propertyIds = properties.ToList();

            //get all property owners in properties under requested estate
            var ownerIds = await _estateRepository.GetPropertiesOwnerById(propertyIds).ConfigureAwait(false);


            //get all property users in properties under requested estate
            var propertyUsers = await _estateRepository.GetPropertyUserIdById(propertyIds).ConfigureAwait(false);

            //combined list of owners and propertyusers
            var combinedUserIds = ownerIds
                                .Concat(propertyUsers)
                                .Distinct()
                                .ToList();

            //get devicetoken for users in that estate
            foreach (var id in combinedUserIds)
            {
                deviceTokens.Add(await _userRepository.GetDeviceToken(id).ConfigureAwait(false));

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
                        var topicResponse = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(deviceTokens, request.Topic);
                    }
                }
            }
            catch (Exception)
            { }


            //send notifications to topics
            if (!string.IsNullOrEmpty(request.Topic))
            {
                response.Message = await _pushNotification.SendMessageToGroupAsync(request.Topic, request.Title, request.Body).ConfigureAwait(false);
                foreach (var id in combinedUserIds)
                {
                    AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                    {
                        UserID = id,
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
