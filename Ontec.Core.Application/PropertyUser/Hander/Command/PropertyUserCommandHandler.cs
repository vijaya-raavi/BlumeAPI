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
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.Estate;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.Notification.Queries;
using Ontec.Core.Domain.Requests.PropertyUser.Command;

namespace Ontec.Core.Application.PropertyUser.Hander.Command
{
    public class PropertyUserCommandHandler : IRequestHandler<DeletePropertyUserById, string>
                                              , IRequestHandler<AddUpdatePropertyUser, AddUpdateResultDto>
                                              , IRequestHandler<DeletePropertyAssociateUserByPropertyId, string>
                                              , IRequestHandler<AddUpdateAssociateUserSettingsQuery, AddUpdateResultDto>

    {
        private readonly IPropertyUserRepository _propertyUserRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IWorkContext _workContext;
        private readonly IOtpRepository _otpRepository;
        private readonly IOtpService _otpService;
        private readonly ICommonService _commonService;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IEncryptionandDecryption _encryptionandDecryption;
        private readonly ICompanyRepository _companyRepository;
        private readonly IEstateRepository _estateRepository;
        private readonly IGenericRepository _genericRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private IHostingEnvironment _environment;
        private readonly ICompanyHelper _companyHelper;
        private readonly IAuditTrail _auditTrail;
        public PropertyUserCommandHandler(IPropertyUserRepository propertyUserRepository
                                          , IPropertyRepository propertyRepository
                                          , IUserRepository userRepository
                                            , IMeterRepository meterRepository
                                            , IWorkContext workContext
                                            , IOtpRepository otpRepository
                                            , ICommonService commonService
                                            , IOtpService otpService
                                            , ICommunicationRepository communicationRepository
                                            , INotificationRepository notificationRepository
                                            , IEncryptionandDecryption encryptionandDecryption
                                            , ICompanyRepository companyRepository
                                             , IEstateRepository estateRepository
                                            , IGenericRepository genericRepository
                                            , IEmailTemplateRepository emailTemplateRepository
                                            , IHostingEnvironment environment
                                            , ICompanyHelper companyHelper
                                            , IAuditTrail auditTrail)
        {
            _propertyUserRepository = propertyUserRepository;
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
            _meterRepository = meterRepository;
            _workContext = workContext;
            _otpRepository = otpRepository;
            _otpService = otpService;
            _commonService = commonService;
            _communicationRepository = communicationRepository;
            _notificationRepository = notificationRepository;
            _encryptionandDecryption = encryptionandDecryption;
            _companyRepository = companyRepository;
            _estateRepository = estateRepository;
            _environment = environment;
            _genericRepository = genericRepository;
            _emailTemplateRepository = emailTemplateRepository;
            _companyHelper = companyHelper;
            _auditTrail = auditTrail;
        }
        public async Task<string> Handle(DeletePropertyUserById request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new DeletePropertyUserByIdValidator(_propertyUserRepository, _propertyRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var objAudit = new AuditHelper();
            var propertyuser = await _propertyUserRepository.GetPropertyUserById(request.Id).ConfigureAwait(false);
            var propertyUserEnumType = (PropertyUserRelationEnum)propertyuser.PropertyUserTypeId;
            string userType = propertyUserEnumType.ToString();
            var user = await _userRepository.GetUserById(propertyuser.UserId).ConfigureAwait(false);

            var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);
            var property = await _propertyRepository.GetPropertyById(propertyuser.PropertyId).ConfigureAwait(false);
            if (user != null && propertyuser.PropertyUserTypeId == (int)PropertyUserRelationEnum.Tenant)
            {
                // await _propertyUserRepository.DeletePropertyUserById(request.Id).ConfigureAwait(false);// deleting one by one property user instead ofd elete user by property id 
                await _propertyUserRepository.DeletePropertyAssociateUserById(propertyuser.PropertyId).ConfigureAwait(false);// deleting Associate on Tenent deleted
                AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                {
                    UserID = _workContext.CurrentUserId,
                    Title = "Property user deleted",
                    Description = userType + " :  " + propertyuser.FirstName + " deleted from " + company.CompanyName + " system for the account " + property.UnitNumber,
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Deleted

                };
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                objAudit.ModifiedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_property_user_relation";
                objAudit.ModuleName = "Property User";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.Action = "Tenant User deletd";
                objAudit.EntityName = "Tenant user deleted from property " + property.Name;
                objAudit.UpdatedId = property.Id;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            if (user != null && propertyuser.PropertyUserTypeId == (int)PropertyUserRelationEnum.Associate)
            {
                await _propertyUserRepository.DeletePropertyUserById(request.Id).ConfigureAwait(false);// deleting one by one property user instead ofd elete user by property id 
                //await _propertyUserRepository.DeletePropertyAssociateUserById(propertyuser.PropertyId).ConfigureAwait(false);// deleting Associate on Tenent deleted
                AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                {
                    UserID = _workContext.CurrentUserId,
                    Title = "Property user deleted",
                    Description = userType + " :  " + propertyuser.FirstName + " deleted from " + company.CompanyName + " system for the account " + property.UnitNumber,
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Deleted

                };
                if (user.StatusId == (int)StatusEnum.Pending && user.RoleId == (int)RoleMasterEnum.Temporary)
                {
                    //await _propertyUserRepository.DeletePermanentPropertyUser(request.Id).ConfigureAwait(false);
                    //await _userRepository.DeleteUserPermanentById(user.Id).ConfigureAwait(false);
                }
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                objAudit.ModifiedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_property_user_relation";
                objAudit.ModuleName = "Property User";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.Action = "Associate User deletd";
                objAudit.EntityName = "Associate user deleted from property " + property.Name;
                objAudit.UpdatedId = property.Id;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            await _propertyUserRepository.DeletePropertyUserById(request.Id).ConfigureAwait(false);
            if (user != null && propertyuser.PropertyUserTypeId == (int)PropertyUserRelationEnum.Associate)
            {
                //await _propertyUserRepository.DeleteAssociateUserSetting(request.Id).ConfigureAwait(false);
                //await _propertyUserRepository.DeletePermanentPropertyUser(request.Id).ConfigureAwait(false);
                await _userRepository.DeleteUserPermanentById(user.Id).ConfigureAwait(false);


            }

            return "Deleted successfully!";
        }

        public async Task<AddUpdateResultDto> Handle(AddUpdatePropertyUser request, CancellationToken cancellationToken)
        {
            var objAudit = new AuditHelper();
            request.TrimAllStrings();
            var commonValidator = new AddUpdatePropertyUserValidator(_propertyUserRepository, _propertyRepository, _userRepository, _meterRepository, _workContext, _communicationRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;
            var propertyUserEnumType = (PropertyUserRelationEnum)request.PropertyUserTypeId;
            string userType = propertyUserEnumType.ToString();
            var userId = await _userRepository.IsMobileInTempUserExist(request.Mobile, request.CompanyId).ConfigureAwait(false);
            var property = await _propertyRepository.GetPropertyById(request.PropertyId).ConfigureAwait(false);
            var user = await _userRepository.GetUserById(_workContext.CurrentUserId).ConfigureAwait(false);
            if (userId == 0)
            {
                //external user
                var userdeto = new UserProfileDto
                {
                    Email = request.Email,
                    Mobile = request.Mobile,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    TitleId = request.TitleId,
                    RoleId = (int)RoleMasterEnum.Temporary,
                    CompanyId = request.CompanyId,
                    StatusId = (int)StatusEnum.Pending,
                    TaxNumber = request.TaxNumber,

                };
                var password = _encryptionandDecryption.Encrypt("Admin@123");
                int propertyusertype = request.PropertyUserTypeId;
                userId = await _userRepository.AddUser(userdeto, password, propertyusertype).ConfigureAwait(false);

                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_property_user_relation";
                objAudit.ModuleName = "Property User";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.Action = userType + " User added for property : " + property.Name;
                objAudit.EntityName = request.FirstName + " " + request.LastName;
                objAudit.UpdatedId = request.PropertyId;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);


                var group = await _notificationRepository.GetGroups().ConfigureAwait(false);
                var newUserGroup = group.FirstOrDefault(g => g.Name.Equals("New Users"));
                List<int> userIds = new List<int>();
                userIds.Add(userId);
                var req = new SubscribeTopicsforUsersRequestQuery()
                {
                    GroupId = newUserGroup.Id,
                    UserIds = userIds
                };
                await _notificationRepository.AddNewUserInCustomersInNotificationTopics(newUserGroup.Id, userId).ConfigureAwait(false);
                if (user.IsEstateEnable == "1" && property.EstateId > 0)
                {
                    var usersIds = new List<int>();
                    int groupId = await _notificationRepository.GetGroupIdByEstateId(property.EstateId).ConfigureAwait(false);
                    usersIds.Add(property.OwnerId);
                    usersIds.Add(userId);
                    var addUserInTopicRequest = new SubscribeTopicsforUsersRequestQuery()
                    {
                        GroupId = groupId,
                        UserIds = usersIds
                    };

                    await _notificationRepository.AddCustomersInNotificationTopics(addUserInTopicRequest).ConfigureAwait(false);
                }
            }

            if (userId > 0)
            {
                var sendNotification = new SendUserNotificationQuery
                {
                    UserId = userId
                };

                await _commonService.SendUserNotificationQuery(sendNotification).ConfigureAwait(false);
                int isPropertyUserInActive = await _propertyUserRepository.IsPropertyUserInActive(request.PropertyId, userId).ConfigureAwait(false);
                request.Id = isPropertyUserInActive;
            }


            if (request.Id > 0)
            {
                //Update user will be not in current scope 
                result = await _propertyUserRepository.UpdatePropertyUser(request, userId).ConfigureAwait(false);
                objAudit.ModifiedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_property_user_relation";
                objAudit.ModuleName = "Property User";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.Action = userType + " User updated for property : " + property.Name;
                objAudit.EntityName = request.FirstName + " " + request.LastName;
                objAudit.UpdatedId = request.PropertyId;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            else
            {
                var serialNumber = await _propertyUserRepository.GetSerialNumber(request.PropertyId).ConfigureAwait(false);
                result = await _propertyUserRepository.AddPropertyUser(request, userId, serialNumber + 1).ConfigureAwait(false);
                //_ = await _otpService.SendInvitationOtp(r, body, request.Mobile).ConfigureAwait(false);
                var userById = await _userRepository.GetNewRegisterUserById(userId).ConfigureAwait(false);
                if (userById.CommunicationTypes == null)
                {
                    //added default commnucation
                    var existingUserCommunications = await _communicationRepository.GetCommunicationMasters().ConfigureAwait(false);
                    var communicationTypes = existingUserCommunications.Where(t => t.Name.Equals("Email")).Select(t => t.Id).AsEnumerable();
                    await _communicationRepository.InsertUserCommunications(communicationTypes, result).ConfigureAwait(false);
                }
                if (user.IsEstateEnable == "1" && property.EstateId > 0)
                {
                    var usersIds = new List<int>();
                    int groupId = await _notificationRepository.GetGroupIdByEstateId(property.EstateId).ConfigureAwait(false);
                    usersIds.Add(property.OwnerId);
                    usersIds.Add(userById.Id);
                    var addUserInTopicRequest = new SubscribeTopicsforUsersRequestQuery()
                    {
                        GroupId = groupId,
                        UserIds = usersIds
                    };

                    await _notificationRepository.AddCustomersInNotificationTopics(addUserInTopicRequest).ConfigureAwait(false);
                }
            }
            // var inviter = await _propertyUserRepository.GetPropertyOwnerDetails(request.PropertyId, _workContext.CurrentUserId).ConfigureAwait(false);
            if (result > 0)
            {
                response.Id = result;


                if (request.Id == 0)
                {
                    response.Message = userType + " user created successfully!";
                }
                else
                {
                    response.Message = userType + " user updated successfully!";
                }
                //var property = await _propertyRepository.GetPropertyById(request.PropertyId).ConfigureAwait(false);
                var inviter = new Domain.Models.Dto.PropertyUser.GetPropertyOwnerDeatilsDto();
                //ToDo: if current user is tenent then pull the details of tenet else pull the details of owner 
                bool pullPropertyOwnerId = false;
                var tenantsCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(request.PropertyId, (int)PropertyUserRelationEnum.Tenant).ConfigureAwait(false);
                if (request.PropertyUserTypeId == (int)PropertyUserRelationEnum.Associate)
                {
                    if (tenantsCount == 0)
                    {
                        pullPropertyOwnerId = true;
                    }
                    else
                    {
                        //get tenant details 
                        inviter = await _propertyUserRepository.GetPropertyTenantDetails(request.PropertyId, _workContext.CurrentUserId).ConfigureAwait(false);
                    }
                }
                else
                {
                    pullPropertyOwnerId = true;
                }
                if (pullPropertyOwnerId)
                    inviter = await _propertyUserRepository.GetPropertyOwnerDetails(request.PropertyId, _workContext.CurrentUserId).ConfigureAwait(false);
                var company = await _companyRepository.GetCompanyDetails(request.CompanyId).ConfigureAwait(false);
                if (inviter != null)
                {
                    EmailModelClass obj = new()
                    {

                        title = "Invitation to Property User",
                        email = request.Email,
                        forEvent = "newPropertyUser",
                        subtitle = "",
                        companyId = request.CompanyId,
                        mobile = request.Mobile,
                        propertyUser = request.FirstName,
                        body = inviter.UnitNumber + " by: " + inviter.PropertyOwner + " for property " + inviter.Property + "!",
                        documentPath = ""

                    };
                    _ = await _otpService.SendEventMail(obj).ConfigureAwait(false);

                    AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                    {
                        UserID = _workContext.CurrentUserId,
                        Title = userType + " registered",
                        Description = userType + " :  " + request.FirstName + " registered in " + company.CompanyName + " system for the account " + inviter.UnitNumber,
                        IsRead = (int)StatusEnum.Sent,
                        NotificationType = (int)NotificationType.Register

                    };
                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                }

            }
            return response;
        }

        #region DeletePropertyAssociateUserByPropertyId
        public async Task<string> Handle(DeletePropertyAssociateUserByPropertyId request, CancellationToken cancellationToken)
        {
            var objAudit = new AuditHelper();

            request.TrimAllStrings();
            var token = "";
            var commonValidator = new DeletePropertyAssociateUserByPropertyIdValidator(_propertyUserRepository, _propertyRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var propertyuser = await _propertyUserRepository.GetPropertyUserByPropertyId(request.PropertyId).ConfigureAwait(false);
            var propertyUserEnumType = (PropertyUserRelationEnum)propertyuser.PropertyUserTypeId;
            string userType = propertyUserEnumType.ToString();
            var user = await _userRepository.GetUserById(propertyuser.UserId).ConfigureAwait(false);
            var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);
            var property = await _propertyRepository.GetPropertyById(propertyuser.PropertyId).ConfigureAwait(false);
            var estate = await _estateRepository.GetEstateById(property.EstateId).ConfigureAwait(false);
            if (user != null && propertyuser.PropertyUserTypeId == (int)PropertyUserRelationEnum.Associate)
            {
                await _propertyUserRepository.DeletePropertyAssociateUserById(request.PropertyId).ConfigureAwait(false);
                objAudit.ModifiedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_property_user_relation";
                objAudit.ModuleName = "Property User";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.Action = "Associate User deletd";
                objAudit.EntityName = property.Name;
                objAudit.UpdatedId = request.PropertyId;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                AddOrUpdateNotificationsQuery newNotification = new()
                {
                    UserID = _workContext.CurrentUserId,
                    Title = "Property user deleted",
                    Description = userType + " :  " + propertyuser.FirstName + " deleted from " + company.CompanyName + " system for the account " + property.UnitNumber,
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Deleted

                };
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            }
            if (user != null)
                token = await _userRepository.GetDeviceToken(user.Id).ConfigureAwait(false);
            if (token != null)
            {
                var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(jsonPath),
                    });
                }
                var staleTokens = new List<string>();
                staleTokens.Add(token);
                await _notificationRepository.DeleteUserFromNotifications(propertyuser.UserId).ConfigureAwait(false);
                var messaging = FirebaseMessaging.DefaultInstance;
                var res = await messaging.UnsubscribeFromTopicAsync(staleTokens, estate.Estate);
            }
            return "Deleted successfully!";
        }
        #endregion

        #region AssociateUserSettings
        public async Task<AddUpdateResultDto> Handle(AddUpdateAssociateUserSettingsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new AddUpdateAssociateUserSettingsQueryValidator(_userRepository, _propertyRepository, _workContext, _propertyUserRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;
            int Id = await _propertyUserRepository.IsAssociateUserSettingsExist(request.PropertyUserId).ConfigureAwait(false);

            request.Id = Id;

            request.Id = Id;
            if (request.IsAllowTopUp)
                objAudit.StatusId = 1;
            else
                objAudit.StatusId = 0;
            if (request.Id > 0)
            {
                result = await _propertyUserRepository.UpdateAssociateUserSettings(request).ConfigureAwait(false);

                objAudit.ModifiedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_associate_user_settings";
                objAudit.ModuleName = "Associate User Settings";
                objAudit.StatusId = objAudit.StatusId;
                objAudit.Action = "Associate user settings updated";
                objAudit.UpdatedId = request.PropertyId;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            else
            {
                var newAssociate = new AddUpdateAssociateUserSettingsQuery
                {
                    IsAllowTopUp = request.IsAllowTopUp,
                    PropertyId = request.PropertyId,
                    PropertyUserId = request.PropertyUserId
                };
                result = await _propertyUserRepository.AddAssociateUserSettings(newAssociate).ConfigureAwait(false);
                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_associate_user_settings";
                objAudit.ModuleName = "Associate User Settings";
                objAudit.StatusId = objAudit.StatusId;
                objAudit.Action = "Associate user settings added";
                objAudit.UpdatedId = request.PropertyId;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Settings added successfully!";
                else
                    response.Message = "Settings updated successfully!";
            }
            return response;
        }
        #endregion


    }
}