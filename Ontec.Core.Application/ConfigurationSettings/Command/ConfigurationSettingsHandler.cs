using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Requests.Configuration.Command;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Command;
using Ontec.Core.Domain.Requests.Consumer.Commands;
using static Ontec.Core.Domain.Models.Dto.STSPurchase.STSPurchasesDto;

namespace Ontec.Core.Application.ConfigurationSettings.Command
{
    public class ConfigurationSettingsHandler : IRequestHandler<AddOrUpdateConfigurationQuery, string>
                                                , IRequestHandler<DeleteConfigurationById, string>
                                                 , IRequestHandler<UpdateAppBackgroundImage, string>
                                                , IRequestHandler<AddUpdateRejectionReasonQuery, AddUpdateResultDto>
                                                , IRequestHandler<UpdateStatusQuery, AddUpdateResultDto>
                                                , IRequestHandler<UpdateBusinessHoursConfigurations, AddUpdateResultDto>
    {
        private readonly IConfigurationRepository _configurationRepo;
        private readonly IWorkContext _workContext;
        private readonly IUserRepository _userRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IOtpService _otpService;
        private readonly IAuditTrail _auditTrail;
        private readonly IConsumerRepository _consumerRepository;
        private IHostingEnvironment _environment;
        private readonly INotificationRepository _notificationRepository;
        public ConfigurationSettingsHandler(IWorkContext workContext, IConfigurationRepository configurationRepo
                                            , IUserRepository userRepository, IDocumentRepository documentRepository
                                            , IMeterRepository meterRepository, ICompanyRepository companyRepository
                                            , IConfigurationRepository configurationRepository
                                            , IOtpService otpService
                                            , IAuditTrail auditTrail
                                            , INotificationRepository notificationRepository
                                            , IConsumerRepository consumerRepository
                                            , IHostingEnvironment environment)
        {
            _workContext = workContext;
            _configurationRepo = configurationRepo;
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _meterRepository = meterRepository;
            _companyRepository = companyRepository;
            _configurationRepository = configurationRepository;
            _otpService = otpService;
            _auditTrail = auditTrail;
            _consumerRepository = consumerRepository;
            _notificationRepository = notificationRepository;
            _environment = environment;
        }
        public async Task<string> Handle(AddOrUpdateConfigurationQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit=new AuditHelper();
            var commonValidator = new AddOrUpdateConfigurationQueryValidator(_workContext, _userRepository, _companyRepository, _configurationRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            string message = "";
            foreach (var configuration in request.Configurations)
            {
                await _configurationRepo.UpdateConfiguration(request).ConfigureAwait(false);
                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.ModifiedBy = _workContext.CurrentUserId;
                    objAudit.ActionTable = "ohd_configuration";
                    objAudit.ModuleName = "Configuration";
                    objAudit.EntityName = "Configuration update to " + configuration.Value;
                    objAudit.StatusId = (int)StatusEnum.Active;
                    objAudit.Action = "Configuration changed to "+ configuration.Value;

                    objAudit.UpdatedId = configuration.Id;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
                message = "General settings updated successfully!";
            }
            return message;
        }

        public async Task<string> Handle(DeleteConfigurationById request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new DeleteConfigurationByIdValidator(_configurationRepo, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            await _configurationRepo.DeleteConfigurationById(request.Id).ConfigureAwait(false);

            return "Deleted successfully!";
        }

        public async Task<string> Handle(UpdateAppBackgroundImage request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new UpdateAppBackgroundImageValidator(_configurationRepo, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var uploadDocDto = new UploadDocumentDto
            {
                UploadFile = request.AppBackgroundImage,
                FileName = request.Id.ToString() + DateTime.UtcNow.ToString("ddMMyyyhhmmss") + Path.GetExtension(request.AppBackgroundImage.FileName)
            };
            var backgroundUrl = await _documentRepository.SaveLogo(uploadDocDto).ConfigureAwait(false);

            await _configurationRepo.UpdateAppBackGroundImage(backgroundUrl, request.Id).ConfigureAwait(false);

            return "Updated successfully";
        }

        public async Task<AddUpdateResultDto> Handle(AddUpdateRejectionReasonQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new AddUpdateRejectionReasonQueryValidator(_configurationRepo, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;
            if (request.Id == 0)
            {
                result = await _configurationRepo.AddRejectionReason(request).ConfigureAwait(false);
                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.Action = "Add Rejection Reason";
                objAudit.ActionTable = "ohd_request_reject_reason";
                objAudit.ModuleName = "Rejction Reason";
                objAudit.StatusId = (int)StatusEnum.Active;
                objAudit.UpdatedId = request.Id;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            else
            {
                result = await _configurationRepo.UpdateRejectionReason(request).ConfigureAwait(false);
                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.Action = "Update Rejection Reason";
                objAudit.ActionTable = "ohd_request_reject_reason";
                objAudit.ModuleName = "Rejction Reason";
                objAudit.StatusId = (int)StatusEnum.Active;
                objAudit.UpdatedId = request.Id;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Rejection reason added successfully!";
                else
                    response.Message = "Rejection reason updated successfully!";
            }
            return response;

        }

        public async Task<AddUpdateResultDto> Handle(UpdateStatusQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new UpdateStatusQueryValidator(_workContext, _userRepository, _meterRepository, _configurationRepo);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;

            result = await _configurationRepo.UpdateStatus(request).ConfigureAwait(false);

            if (request.UpdateTo == (int)UpdateStatusEnum.User && request.StatusId == (int)StatusEnum.Inactive)
            {

                var user = await _userRepository.GetUserById(request.Id).ConfigureAwait(false);
                // if consumer deleted then removed same consumer from notification groups linked to user id
                var groupLinkingData = await _notificationRepository.GetConsumerWiseGroupLinking(request.Id).ConfigureAwait(false);
                foreach (var consumerLink in groupLinkingData)
                {
                    var reqRemoveLinking = new RemoveConsumerFromGroupQueryRequest
                    {
                        NotificationGroupLinkId = consumerLink.Id,
                        GroupId = consumerLink.GroupId,
                        UserId = request.Id,

                    };
                    var staleToken = new List<string>();
                    int rmId = await _consumerRepository.RemoveConsumerFromGroup(reqRemoveLinking.NotificationGroupLinkId).ConfigureAwait(false);
                    var topic = await _notificationRepository.GetTopicName(reqRemoveLinking.GroupId).ConfigureAwait(false);
                    var tokens = await _userRepository.GetDeviceToken(reqRemoveLinking.UserId).ConfigureAwait(false);
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
                        var resNotification = await messaging.UnsubscribeFromTopicAsync(staleToken, topic);
                    }

                }
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
            }

           

            if (result > 0)
            {
                response.Id = result;
                response.Message = "Status updated successfully!";
            }
            return response;

        }
        public async Task<AddUpdateResultDto> Handle(UpdateBusinessHoursConfigurations request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new UpdateBusinessHoursConfigurationsValidator(_workContext, _userRepository, _companyRepository, _configurationRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();


            await _configurationRepo.UpdateBusinessConfiguration(request).ConfigureAwait(false);
            response.Message = "Business hours  settings updated successfully!";

            return response;
        }
    }
}
