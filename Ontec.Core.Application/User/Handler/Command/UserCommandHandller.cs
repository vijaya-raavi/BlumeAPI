using System.Text.RegularExpressions;
using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.Otp;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.User.Commands;

namespace Ontec.Core.Application.User.Handler.Command
{
    public class UserCommandHandller : IRequestHandler<UpdateUserCommand, AddUpdateResultDto>
                                        , IRequestHandler<UpdateProfileCommand, string>
                                        , IRequestHandler<DeleteUserById, string>
                                        , IRequestHandler<SaveUserSettingsQuery, AddUpdateResultDto>
                                        , IRequestHandler<ChangePasswordQuery, AddUpdateResultDto>
                                        , IRequestHandler<UpdateUserDocumentQuery, AddUpdateResultDto>
                                        , IRequestHandler<UpdateUserContactDeatilsRequestQuery, AddUpdateResultDto>
                                        , IRequestHandler<VerifyUserQueryRequest, AddUpdateResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IWorkContext _workContext;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IEncryptionandDecryption _encryptionandDecryption;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IOtpService _otpService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IGenericRepository _genericRepository;
        private readonly IAuditTrail _auditTrail;
        public UserCommandHandller(IUserRepository userRepository
                                   , ICommunicationRepository communicationRepository
                                   , IDocumentRepository documentRepository
                                   , IWorkContext workContext
                                   , IUserRoleRepository userRoleRepository
                                   , IEncryptionandDecryption encryptionandDecryption
                                   , IConfigurationRepository configurationRepository
                                    , IOtpService otpService
                                   , ICompanyRepository companyRepository
                                   , INotificationRepository notificationRepository
                                    , IOtpRepository otpRepository
                                   , IEmailTemplateRepository emailTemplateRepository
                                   ,IGenericRepository genericRepository,
IAuditTrail auditTrail)
        {
            _userRepository = userRepository;
            _communicationRepository = communicationRepository;
            _documentRepository = documentRepository;
            _workContext = workContext;
            _userRoleRepository = userRoleRepository;
            _encryptionandDecryption = encryptionandDecryption;
            _configurationRepository = configurationRepository;
            _companyRepository = companyRepository;
            _otpService = otpService;
            _notificationRepository = notificationRepository;
            _otpRepository = otpRepository;
            _emailTemplateRepository = emailTemplateRepository;
            _genericRepository = genericRepository;
            _auditTrail = auditTrail;
        }
        public async Task<AddUpdateResultDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new UpdateUserCommandValidator(_userRepository, _communicationRepository, _documentRepository, _userRoleRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var id = 0;
            int Status = 0;
            bool isBusinessHours = false;
            try
            {
                var date = DateTime.UtcNow.Date.Day;
                int proofDocumentId = 0;
                var user = await _userRepository.GetUserById(request.Id).ConfigureAwait(false);
                bool isVerified = user.IsVerified;
                if (request.ProofDocument != null)
                {
                    var uploadDocDto = new UploadDocumentDto
                    {
                        UploadFile = request.ProofDocument,
                        DocumentTypeId = request.ProofDocumentTypeId,
                        //FileName = request.ProofDocument.FileName,
                        FileName = request.Id + "_" + date + "_consumer_identity" +Path.GetExtension(request.ProofDocument.FileName),
                        Id = proofDocumentId,
                        Title = request.ProofDocument.FileName,
                        DocumentNumber = request.DocumentNumber

                    };
                    proofDocumentId = await _documentRepository.UploadDocument(uploadDocDto).ConfigureAwait(false);
                }
                else
                {
                    proofDocumentId = user.ProofDocumentId;
                }
                request.StatusId = (int)StatusEnum.Active;
                if (!user.IsVerified)
                {
                    if ((string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || !user.CommunicationTypesIds.Any()) && user.StatusId == (int)StatusEnum.InProcess)
                    {
                        string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
                        string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;


                        string timeZoneId = "South Africa Standard Time"; // Windows ID
                        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                        DateTime harareTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);


                        formattedTime = formattedTime.Replace('.', ':');
                        var fromBusinesHoursValue = "";
                        var toBusinesHoursValue = "";
                        TimeSpan fromBusinessHours = TimeSpan.Zero;
                        TimeSpan toBusinessHours = TimeSpan.Zero;
                        bool isEnableNonBusinessHours = false;
                        bool isEnableBusinessHours = false;
                        bool enabled = false;
                        var businessConfigurations = await _configurationRepository.GetBusinessHoursConfigurations().ConfigureAwait(false);

                        var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                        var day = System.DateTime.Now.ToString("dddd");

                        if (businessConfigurations != null && businessConfigurations.Any(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)))
                        {

                            fromBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).From;

                            toBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).To;

                            isEnableBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).AcceptUserBusiness;

                            enabled = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).Enabled;

                            isEnableNonBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).AcceptUserNonBusiness;


                        }

                        //if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("frombusinesshours")))
                        //{
                        //    var fromBusinesHours = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("frombusinesshours"));
                        //    if (fromBusinesHours != null && !string.IsNullOrEmpty(fromBusinesHours.Value))
                        //    {
                        //        fromBusinesHoursValue = fromBusinesHours.Value;
                        //    }
                        //}
                        //if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("tobusinesshours")))
                        //{
                        //    var toBusinessHours = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("tobusinesshours"));
                        //    if (toBusinessHours != null && !string.IsNullOrEmpty(toBusinessHours.Value))
                        //    {
                        //        toBusinesHoursValue = toBusinessHours.Value;
                        //    }
                        //}
                        DateTime currentdate = Convert.ToDateTime(harareTime).ToUniversalTime();
                        TimeSpan currentTime = harareTime.TimeOfDay;

                        if (currentTime >= fromBusinessHours && currentTime <= toBusinessHours)
                        {
                            isBusinessHours = true;
                        }
                        else
                        {

                            isBusinessHours = false;
                        }

                        if (enabled && isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval..";
                            isVerified = false;
                        }
                        if (enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }
                        if (enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval.";
                            isVerified = false;
                        }
                        if (enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }


                        if (enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval.";
                            isVerified = false;
                        }
                        if (enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval.";
                            isVerified = false;
                        }
                        if (enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                        {

                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }
                        if (enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }


                        if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval.";
                            isVerified = false;
                        }
                        if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval..";
                            isVerified = false;
                        }
                        if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                        {

                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }
                        if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }

                        if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval.";
                            isVerified = false;
                        }
                        if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Pending;
                            request.Comments = "Your registration request is send to admin for approval.";
                            isVerified = false;
                        }
                        if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }
                        if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }

                        if (!enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                        {
                            request.StatusId = (int)StatusEnum.Active;
                            request.Comments = "";
                            isVerified = false;
                        }


                    }
                }
                if (request.TaxNumber == null)
                    request.TaxNumber = user.TaxNumber;
                var version = user.TermsConditionsCurrentVersion;

                if (string.IsNullOrEmpty(request.TermConditionsVersion) || request.TermConditionsVersion == "string")
                {
                    request.TermConditionsVersion = user.TermsConditionsCurrentVersion;

                }
                id = await _userRepository.UpdateUser(request, proofDocumentId, isVerified);

                Status = request.StatusId;

            }
            catch (Exception ex)
            {
                throw;
            }

            var result = new AddUpdateResultDto()
            {
                Id = id
            };
            if (id > 0)
            {

                var company = await _companyRepository.GetCompanyDetails(request.CompanyId).ConfigureAwait(false);
                AddOrUpdateNotificationsQuery newNotification = new()
                {
                    UserID = request.Id,
                    Title = "User updated",
                    Description = " User updated in " + company.CompanyName,
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Updated

                };
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                result.Message = "User updated successfully!";
                result.StatusId = Status;
            }
            else
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(UpdateUserCommand.Email),
                    ErrorMessage = "Something went wrong."
                });
            }
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            if (result.StatusId != 0)
            {
                var enumStatus = (StatusEnum)result.StatusId;
                result.Status = enumStatus.ToString();
            }
            return result;
        }

        public async Task<string> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new UpdateProfileCommandValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var date = DateTime.UtcNow.Date.Day;
            var uploadDocDto = new UploadDocumentDto
            {
                UploadFile = request.ProfilePicture,
                //FileName = request.Id.ToString() + Path.GetExtension(request.ProfilePicture.FileName)
                FileName = request.Id + "_" + date + "_consumer_prodile_pic" + Path.GetExtension(request.ProfilePicture.FileName),
            };
            var profileUrl = await _documentRepository.SaveDocument(uploadDocDto, true).ConfigureAwait(false);

            await _userRepository.UpdateUserPofilePic(profileUrl, request.Id).ConfigureAwait(false);
            var user = await _userRepository.GetUserById(request.Id).ConfigureAwait(false);
            var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);
            AddOrUpdateNotificationsQuery newNotification = new()
            {
                UserID = request.Id,
                Title = "User updated",
                Description = " User profile picture updated in " + company.CompanyName,
                IsRead = (int)StatusEnum.Sent,
                NotificationType = (int)NotificationType.Updated

            };
            await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            return "Updated successfully";
        }


        #region DeleteUserbyId
        public async Task<string> Handle(DeleteUserById request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new DeleteUserByIdValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            await _userRepository.DeleteUserById(request.Id).ConfigureAwait(false);
            string body;
            string title;
            string subtitle;
            string forEvent;
            var user = await _userRepository.GetUserById(request.Id).ConfigureAwait(false);

            EmailModelClass obj = new()
            {

                title = "Account deleted.",
                email = user.Email,
                forEvent = "AccountDelete",
                subtitle = "",
                companyId = user.CompanyId,
                mobile = user.Mobile,
                propertyUser = user.FirstName,
                body = "",
                documentPath = ""

            };
            await _otpService.SendEventMail(obj).ConfigureAwait(false);
            await _notificationRepository.DeleteUserFromNotifications(request.Id).ConfigureAwait(false);

            return "Deleted successfully!";
        }
        #endregion

        #region SaveUserSettings
        public async Task<AddUpdateResultDto> Handle(SaveUserSettingsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new SaveUserSettingsQueryValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            int result;

            result = await _userRepository.SaveUserSettings(request).ConfigureAwait(false);

            if (result > 0)
            {
                response.Id = result;
                response.Message = "User Settings saved successfully!";
            }
            return response;
        }
        #endregion

        #region changePassword
        public async Task<AddUpdateResultDto> Handle(ChangePasswordQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new ChangePasswordQueryValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            int result = 0;

            var encryptedPassword = _encryptionandDecryption.Encrypt(request.OldPassword);
            request.OldPassword = encryptedPassword;

            var isExist = await _userRepository.IsOldPasswordExist(request).ConfigureAwait(false);

            if (isExist)
            {
                var encryptedNewPassword = _encryptionandDecryption.Encrypt(request.NewPassword);
                request.NewPassword = encryptedNewPassword;
                result = await _userRepository.ChangePassword(request).ConfigureAwait(false);
                if (result > 0)
                {
                    response.Message = "Password updated successfully!";
                    var user = await _userRepository.GetUserById(request.UserId).ConfigureAwait(false);
                    var commId = await _communicationRepository.GetCommunicationsByUserId(user.Id);
                    AddOrUpdateNotificationsQuery newNotification = new()
                    {
                        UserID = request.UserId,
                        Title = "Password Change",
                        Description = " Password changed for user name : " + user.UserName,
                        IsRead = (int)StatusEnum.Sent,
                        NotificationType = (int)NotificationType.Updated

                    };
                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

                    var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
                    var passwordEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Password Reset"));
                    if (!string.IsNullOrEmpty(passwordEmail.Html))
                    {
                        string htmlTemplate = passwordEmail.Html;
                        var matches = Regex.Matches(passwordEmail.Html, @"{{(.*?)}}");
                        List<string> placeholders = matches.Cast<Match>()
                                                .Select(m => m.Groups[1].Value) // Group[1] is the captured variable name
                        .Distinct()
                                                .ToList();
                        var userDict = _genericRepository.ToDictionary(user);

                        foreach (var key in placeholders)
                        {
                            if (userDict.TryGetValue(key, out var value))
                            {
                                passwordEmail.Html = passwordEmail.Html.Replace("{{" + key + "}}", user.FirstName);
                            }
                        }
                        EmailModelClass obj = new()
                        {

                            title = "",
                            email = user.Email,
                            forEvent = "PasswordChanged",
                            subtitle = "",
                            mobile = user.Mobile,
                            propertyUser = user.UserName,
                            body = passwordEmail.Html,
                            documentPath = "",
                            companyId = user.CompanyId
                        };
                        if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
                        {
                            await _otpService.SendEventMail(obj).ConfigureAwait(false);
                        }
                    }
                    int count = await _userRepository.IsSessionKeyExist(request.SessionKey).ConfigureAwait(false);
                    if (count > 0)
                    {
                        await _userRepository.UpdateUserLogOutDeatils(request.UserId, request.SessionKey).ConfigureAwait(false);
                    }
                }

            }
            else
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(ChangePasswordQuery.OldPassword),
                    ErrorMessage = " Old Password does not match with existing system."
                });
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);
            }
            response.Id = result;
            return response;
        }
        #endregion

        #region UpdateUserdocument
        public async Task<AddUpdateResultDto> Handle(UpdateUserDocumentQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new UpdateUserDocumentQueryValidator(_userRepository, _documentRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var id = 0;
            try
            {
                var date = DateTime.UtcNow.Date.Day;

                var user = await _userRepository.GetUserById(request.UserId).ConfigureAwait(false);
                int proofDocumentId = user.ProofDocumentId;
                if (request.ProofDocument != null)//&& user.StatusId == (int)StatusEnum.Pending)
                {
                    var uploadDocDto = new UploadDocumentDto
                    {
                        UploadFile = request.ProofDocument,
                        DocumentTypeId = request.ProofDocumentTypeId,
                        FileName = request.UserId + "_" + date + "_consumer_identity" + Path.GetExtension(request.ProofDocument.FileName),
                        Id = proofDocumentId,
                        Title = request.ProofDocument.FileName,
                        DocumentNumber = request.DocumentNumber
                    };
                    proofDocumentId = await _documentRepository.UploadDocument(uploadDocDto).ConfigureAwait(false);
                }
                else
                {
                    proofDocumentId = user.ProofDocumentId;
                }
                id = await _userRepository.UpdateUserDocument(request, proofDocumentId);
            }
            catch (Exception ex)
            {
                throw;
            }
            var result = new AddUpdateResultDto()
            {
                Id = id
            };
            if (id > 0)
                result.Message = "User document updated successfully!";
            else
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(UpdateUserDocumentQuery.ProofDocument),
                    ErrorMessage = "Something went wrong."
                });
            }
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return result;
        }

        #endregion

        public async Task<AddUpdateResultDto> Handle(UpdateUserContactDeatilsRequestQuery request, CancellationToken cancellationToken)
        {
            var objAudit = new AuditHelper();
            request.TrimAllStrings();
            var res = new AddUpdateResultDto();
            var commonValidator = new UpdateUserContactDeatilsRequestQueryValidator(_workContext, _userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            bool isActiveUserExist = false;
            if (request.IsEmail && !string.IsNullOrEmpty(request.EmailId))
            {
                isActiveUserExist = await _userRepository.IsUserByEmailMobileActive(request.EmailId);
            }
            if (!request.IsEmail && !string.IsNullOrEmpty(request.MobileNumber))
            {
                isActiveUserExist = await _userRepository.IsUserByEmailMobileActive(request.MobileNumber);
            }
            var getUser = new UserProfileDto();
            var email = "";
            var mobile = "";
            var otpModel = new OtpModel();
            getUser = await _userRepository.GetUserById(request.UserId).ConfigureAwait(false);
            if (request.IsEmail)
            {
                mobile = getUser.Mobile;
                otpModel = await _otpRepository.GetOtpByMobileNumberCompanyId(getUser.Mobile, _workContext.CurrentCompanyId, request.EmailId, (int)StatusEnum.NotVerified).ConfigureAwait(false);
            }
            if (!request.IsEmail)
            {
                email = getUser.Email;
                otpModel = await _otpRepository.GetOtpByMobileNumberCompanyId(request.MobileNumber, _workContext.CurrentCompanyId, getUser.Email, (int)StatusEnum.NotVerified).ConfigureAwait(false);
            }

            if (otpModel == null)
            {
                res.Message = "Your OTP is expired.";
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(RegisterUserCommand.Otp),
                    ErrorMessage = res.Message,
                });
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);
            }
            DateTime nowTime = DateTime.UtcNow;
            TimeSpan span = nowTime.Subtract(otpModel.CreatedAt);
            int otpDuration = span.Minutes;
            if (otpDuration > 10)
            {
                res.Message = "Your OTP is expired.";
                await _otpRepository.UpdateVerifiedOtp(otpModel.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(UpdateUserContactDeatilsRequestQuery.Otp),
                    ErrorMessage = res.Message,
                });
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);

            }
            else if (otpModel != null && otpModel.Otp.Trim() != request.Otp)
            {
                res.Message = "Incorrect OTP.";
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(RegisterUserCommand.Otp),
                    ErrorMessage = res.Message,

                });
               
            }
            else
            {

                if (otpModel != null && otpModel.Otp.Trim() == request.Otp)
                {
                    await _otpRepository.UpdateVerifiedOtp(otpModel.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);

                    request.CountryCodeId = await _otpRepository.GetCountryCodeId(request.CountryCode, (int)StatusEnum.Active).ConfigureAwait(false);
                    if (request.IsEmail)
                    {
                        res.Id = await _userRepository.UpdateUserContacts(request.EmailId, request.UserId, request.IsEmail).ConfigureAwait(false);
                        if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                        {
                            objAudit.ModifiedBy = _workContext.CurrentUserId;
                            objAudit.ActionTable = "ohd_user";
                            objAudit.ModuleName = "Consumer Primary Deatils";

                            objAudit.StatusId = (int)StatusEnum.Active;
                            objAudit.Action = "Email Changed To: " + request.EmailId;

                            objAudit.UpdatedId = request.UserId;
                            await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                        }
                    }
                    if (!request.IsEmail)
                    {
                        res.Id = await _userRepository.UpdateUserContacts(request.MobileNumber, request.UserId, request.IsEmail).ConfigureAwait(false);
                        if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                        {
                            objAudit.ModifiedBy = _workContext.CurrentUserId;
                            objAudit.ActionTable = "ohd_user";
                            objAudit.ModuleName = "Consumer Primary Deatils";

                            objAudit.StatusId = (int)StatusEnum.Active;
                            objAudit.Action = "Mobile No Changed To: " + request.MobileNumber;

                            objAudit.UpdatedId = request.UserId;
                            await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                        }
                    }

                    if (res.Id > 0)
                    {
                        var user = await _userRepository.GetUserById(res.Id).ConfigureAwait(false);
                        //mail send 
                        var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);
                        string body;
                        string title;
                        string subtitle;
                        string forEvent;
                        if (request.IsEmail)
                        {
                            title = "Email updated successfully ";
                            res.Message = "Email updated successfully ";
                        }
                        else
                        {
                            title = "Mobile number updated successfully ";
                            res.Message = "Mobile number updated successfully ";
                        }
                        AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                        {
                            UserID = res.Id,
                            Title = title,
                            Description = title + company.CompanyName,
                            IsRead = (int)StatusEnum.Sent,
                            NotificationType = (int)NotificationType.Register

                        };
                        await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                        subtitle = " ";
                        forEvent = "ContactUpdate";

                        EmailModelClass obj = new()
                        {
                            title = title,
                            email = user.Email,
                            forEvent = "ContactUpdate",
                            subtitle = "",
                            mobile = user.Mobile,
                            propertyUser = user.FirstName,
                            body = "Your " + title + company.CompanyName,
                            documentPath = "",
                            companyId = user.CompanyId



                        };
                        await _otpService.SendEventMail(obj).ConfigureAwait(false);

                    }
                    else
                    {
                        res.Message = "Incorrect OTP.";
                        validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                        {
                            PropertyName = nameof(RegisterUserCommand.Otp),
                            ErrorMessage=res.Message,
                        });
                        if (!validatorResult.IsValid)
                            throw new ValidationException(validatorResult.Errors);
                    }


                }

            }
            return res;
        }
        public async Task<AddUpdateResultDto> Handle(VerifyUserQueryRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var res = new AddUpdateResultDto();
            var commonValidator = new VerifyUserQueryRequestValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            res.Id = await _userRepository.VeirfyUserById(request.UserId).ConfigureAwait(false);
            if (res.Id > 0)
            {
                res.Message = "User verified successfully!";
            }
            return res;
        }
    }
}