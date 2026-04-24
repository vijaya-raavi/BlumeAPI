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
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Requests.Consumer.Commands;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.User.Queries;
using Scriban;

namespace Ontec.Core.Application.Consumer.Commands
{
    public class UpdateConsumerHandler : IRequestHandler<UpdateConsumerQuery, AddUpdateResultDto>,
                                         IRequestHandler<DeleteConsumerById, string>,
                                         IRequestHandler<ApproveRejectRegistrtionRequestQuery, AddUpdateResultDto>,
                                         IRequestHandler<RemoveConsumerFromGroupQueryRequest, AddUpdateResultDto>,
                                        IRequestHandler<SendNotifcationToConsumerRequest, PushNotificationDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IConsumerRepository _consumerRepository;
        private readonly IPushNotification _pushNotification;
        private readonly ICompanyRepository _companyRepository;
        private readonly IOtpService _otpService;
        private readonly IWorkContext _workContext;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICompanyHelper _companyHelper;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private IHostingEnvironment _environment;
        private readonly IAuditTrail _auditTrail;
        public UpdateConsumerHandler(IUserRepository userRepository
                                    , IConsumerRepository consumerRepository
                                    , IPushNotification pushNotification
                                    , ICompanyRepository companyRepository
                                    , IOtpService otpService
                                    , IWorkContext workContext
                                    , INotificationRepository notificationRepository
                                    , ICompanyHelper companyHelper
                                    , IEmailTemplateRepository emailTemplateRepository
                                    , IHostingEnvironment environment
                                    , IAuditTrail auditTrail)
        {

            _userRepository = userRepository;
            _consumerRepository = consumerRepository;
            _pushNotification = pushNotification;
            _companyRepository = companyRepository;
            _otpService = otpService;
            _workContext = workContext;
            _notificationRepository = notificationRepository;
            _companyHelper = companyHelper;
            _emailTemplateRepository = emailTemplateRepository;
            _environment = environment;
            _auditTrail = auditTrail;
        }
        public async Task<AddUpdateResultDto> Handle(UpdateConsumerQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
           
            var commonValidator = new UpdateConsumerQueryValidator(_consumerRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;
            result = await _consumerRepository.UpdateConsumer(request).ConfigureAwait(false);


            if (result > 0)
            {

                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Something Went wrong!";
                else
                    response.Message = "Records updated successfully!";
            }
            return response;
        }
        public async Task<string> Handle(DeleteConsumerById request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new DeleteConsumerByIdValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            await _consumerRepository.DeleteConsumerById(request.Id).ConfigureAwait(false);
            var user = await _userRepository.GetUserById(request.Id).ConfigureAwait(false);
            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.Action = "Delete";
                objAudit.ActionTable = "ohd_user";
                objAudit.ModuleName = "Consumer";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.UpdatedId = request.Id;
                objAudit.EntityName = user.FirstName + ' ' + user.LastName;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            EmailModelClass obj = new()
            {

                title = "Account deleted.",
                email = user.Email,
                forEvent = "ConsumerDelete",
                subtitle = "",
                companyId = user.CompanyId,
                mobile = user.Mobile,
                propertyUser = user.FirstName,
                body = "",
                documentPath = ""

            };
            await _otpService.SendEventMail(obj).ConfigureAwait(false);
            return "Deleted successfully!";
        }

        #region GetApproveRejectRegRequest
        public async Task<AddUpdateResultDto> Handle(ApproveRejectRegistrtionRequestQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new GetApproveRejectRegistrtionRequestQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            int result;

            result = await _userRepository.ApproveRejectRegistrationRequestById(request).ConfigureAwait(false);
            var user = await _userRepository.GetUserById(request.ID).ConfigureAwait(false);
            string deviceToken = await _userRepository.GetDeviceToken(request.ID).ConfigureAwait(false);
            var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);
            var companyDetails = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
            var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
            var welcomeEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Reject User"));
            if (!string.IsNullOrEmpty(welcomeEmail.Html))
            {
                var model = new PropertyUserWelcomeEmailDto
                {
                    CompanyName = companyDetails.Name,
                    FirstName = user.FirstName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    companyEmail = companyDetails.Email,
                    Domain = companyDetails.Domain,
                    companyLogo = companyDetails.RelativeUrl,
                    RejectReason = request.Comments
                };
                var template = Template.Parse(welcomeEmail.Html);
                welcomeEmail.Html = template.Render(model, memberRenamer: member => member.Name);
            }

            if (result > 0)
            {
                welcomeEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Approve User"));
                string title = "";
                string body = "";
                if (request.IsApproved)
                {

                    if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                    {
                        objAudit.AddedBy = _workContext.CurrentUserId;
                        objAudit.Action = "Approve Consumer";
                        objAudit.ActionTable = "ohd_user";
                        objAudit.ModuleName = "Consumer";
                        objAudit.StatusId = (int)StatusEnum.Active;
                        objAudit.UpdatedId = request.ID;
                        await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                    }


                    title = "Your registration request is approved";
                    if (!string.IsNullOrEmpty(welcomeEmail.Html))
                    {
                        var model = new PropertyUserWelcomeEmailDto
                        {
                            CompanyName = companyDetails.Name,
                            FirstName = user.FirstName,
                            Email = user.Email,
                            Mobile = user.Mobile,
                            companyEmail = companyDetails.Email,
                            Domain = companyDetails.Domain,
                            companyLogo = companyDetails.RelativeUrl
                        };
                        var template = Template.Parse(welcomeEmail.Html);
                        welcomeEmail.Html = template.Render(model, memberRenamer: member => member.Name);
                    }
                    EmailModelClass obj = new()
                    {

                        title = "Your registration request is approved",
                        email = user.Email,
                        forEvent = "ApproveUser",
                        subtitle = "",
                        companyId = user.CompanyId,
                        mobile = user.Mobile,
                        propertyUser = user.UserName,
                        // body = "Welcome to " + company.CompanyName,
                        body = welcomeEmail.Html,
                        documentPath = ""
                    };

                    if (user != null && !string.IsNullOrEmpty(deviceToken))
                    {

                        await _pushNotification.SendMessage(title, body, deviceToken, user.Id).ConfigureAwait(false);

                    }
                    if (user != null)
                    {
                        AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                        {
                            UserID = user.Id,
                            Title = "Your registration request is approved",
                            Description = "Welcome to " + company.CompanyName,
                            IsRead = (int)StatusEnum.Sent,
                            NotificationType = (int)NotificationType.Updated,

                        };
                        await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                    }
                    await _otpService.SendEventMail(obj).ConfigureAwait(false);
                }
                else
                {
                    if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                    {
                        objAudit.ModifiedBy = _workContext.CurrentUserId;
                        objAudit.Action = "Reject Consumer";
                        objAudit.ActionTable = "ohd_user";
                        objAudit.ModuleName = "Consumer";
                        objAudit.StatusId = (int)StatusEnum.Rejected;
                        objAudit.UpdatedId = request.ID;
                        await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                    }
                    title = "Your registration request is rejected";
                    body = "Your registration request to " + company.CompanyName + " is rejected.Reason : " + request.Comments + ".Kindly contact to administration for more details.";
                    EmailModelClass obj = new()
                    {

                        title = "Your registration request is rejected",
                        email = user.Email,
                        forEvent = "RejectUser",
                        subtitle = request.Comments,
                        companyId = user.CompanyId,
                        mobile = user.Mobile,
                        propertyUser = user.UserName,
                        //body = "Your registration request to company, " + company.CompanyName + " was rejected.",
                        body = welcomeEmail.Html,
                        documentPath = ""

                    };
                    if (user != null && !string.IsNullOrEmpty(deviceToken))
                    {

                        await _pushNotification.SendMessage(title, body, deviceToken, user.Id).ConfigureAwait(false);
                    }

                    await _otpService.SendEventMail(obj).ConfigureAwait(false);
                }

                response.Id = result;
                if (request.ID == 0)
                    response.Message = "Something Went wrong!";
                else

                    response.Message = "Customer status updated successfully!";
            }
            return response;
        }
        #endregion


        public async Task<AddUpdateResultDto> Handle(RemoveConsumerFromGroupQueryRequest request, CancellationToken cancellationToken)
        {
            var staleToken = new List<string>();
            var res = new AddUpdateResultDto();
            request.TrimAllStrings();
            var commonValidator = new RemoveConsumerFromGroupQueryRequestValidator(_consumerRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            res.Id = await _consumerRepository.RemoveConsumerFromGroup(request.NotificationGroupLinkId).ConfigureAwait(false);
            var topic = await _notificationRepository.GetTopicName(request.GroupId).ConfigureAwait(false);
            var tokens = await _userRepository.GetDeviceToken(request.UserId).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(tokens))
            {
                staleToken.Add(tokens);
                var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(jsonPath),
                    });
                }
                var messaging = FirebaseMessaging.DefaultInstance;
                var response = await messaging.UnsubscribeFromTopicAsync(staleToken, topic);
            }
            if (res.Id > 0)
            {
                res.Message = "Consumer removed from group successfully";
            }
            return res;
        }
        public async Task<PushNotificationDto> Handle(SendNotifcationToConsumerRequest request, CancellationToken cancellationToken)
        {
            var res = new PushNotificationDto();
            request.TrimAllStrings();
            var commonValidator = new SendNotifcationToConsumerRequestValidator(_consumerRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var deviceToken = await _userRepository.GetDeviceToken(request.ConsumerId).ConfigureAwait(false);
            res = await _pushNotification.SendMessage(request.Title, request.Body, deviceToken, request.ConsumerId).ConfigureAwait(false);
            AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
            {
                UserID = request.ConsumerId,
                Title = request.Title,
                Description = request.Body,
                IsRead = (int)StatusEnum.Sent,
                NotificationType = (int)NotificationType.Updated,

            };
            await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            return res;
        }
   
    }
}