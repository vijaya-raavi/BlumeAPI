using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.User.Queries;
using Scriban;

namespace Ontec.Core.Application.Login.Handler.Command
{
    public class LoginCommandHandler : IRequestHandler<RegisterUserCommand, LoginResult>,
                                    IRequestHandler<ResetPassword, AddUpdateResultDto>,
                                    IRequestHandler<LogOutRequestQuery, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IWorkContext _workContext;
        private readonly IOtpRepository _otpRepository;
        private readonly IOtpService _otpService;
        private readonly IEncryptionandDecryption _encryptionandDecryption;
        private readonly IWalletRepository _walletRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IGenericRepository _genericRepository;
        private readonly ICompanyHelper _companyHelper;

        public LoginCommandHandler(IUserRepository userRepository
            , ICommunicationRepository communicationRepository      
                                   , IWorkContext workContext
                                   , IOtpRepository otpRepository
                                   , IEncryptionandDecryption encryptionandDecryption
                                   , IWalletRepository walletRepository
                                   , IOtpService otpService
                                   , INotificationRepository notificationRepository
                                   , ICompanyRepository companyRepository
                                   , IEmailTemplateRepository emailTemplateRepository
                                   , IGenericRepository genericRepository
                                   , ICompanyHelper companyHelper)
        {
            _userRepository = userRepository;
            _workContext = workContext;
            _otpRepository = otpRepository;
            _encryptionandDecryption = encryptionandDecryption;
            _walletRepository = walletRepository;
            _otpService = otpService;
            _notificationRepository = notificationRepository;
            _companyRepository = companyRepository;
            _emailTemplateRepository = emailTemplateRepository;
            _genericRepository = genericRepository;
            _communicationRepository = communicationRepository;
            _companyHelper = companyHelper;

        }

        public async Task<LoginResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new RegisterUserCommandValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            request.Password = _encryptionandDecryption.Encrypt(request.Password);

            var isActiveUserExist = await _userRepository.IsUserByEmailMobileActive(request.EmailId);
            var otpModel = await _otpRepository.GetOtpByMobileNumberCompanyId(request.MobileNumber, request.CompanyId, request.EmailId, (int)StatusEnum.NotVerified).ConfigureAwait(false);
            if (otpModel == null)
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(RegisterUserCommand.Otp),
                    ErrorMessage = "Your OTP is expired."
                });
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);
            }
            DateTime nowTime = DateTime.UtcNow;
            TimeSpan span = nowTime.Subtract(otpModel.CreatedAt);
            int otpDuration = span.Minutes;
            if (otpDuration > 10)
            {
                await _otpRepository.UpdateVerifiedOtp(otpModel.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(RegisterUserCommand.Otp),
                    ErrorMessage = "Your OTP is expired."
                });
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);

            }
            else if (otpModel != null && otpModel.Otp.Trim() != request.Otp)
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(RegisterUserCommand.Otp),
                    ErrorMessage = "Incorrect OTP."
                });
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);
            }
            else
            {
                string devicetoken = "";
                if (!isActiveUserExist)
                {
                    if (otpModel != null && otpModel.Otp.Trim() == request.Otp)
                    {
                        await _otpRepository.UpdateVerifiedOtp(otpModel.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);

                        request.CountryCodeId = await _otpRepository.GetCountryCodeId(request.CountryCode, (int)StatusEnum.Active).ConfigureAwait(false);
                        var userId = await _userRepository.IsUserTemporary(request.EmailId).ConfigureAwait(false);
                        if (userId > 0)
                        {
                            userId = await _userRepository.UpdateTemporaryUser(request).ConfigureAwait(false);
                        }
                        else
                            userId = await _userRepository.RegisterUser(request);

                        //for insert deviceToken
                        if (!string.IsNullOrEmpty(request.Devicetoken))
                        {

                            devicetoken = await _userRepository.GetDeviceToken(userId).ConfigureAwait(false);
                            if (string.IsNullOrEmpty(devicetoken) && request.Devicetoken != "string")
                            {
                                await _userRepository.InsertDeviceToken(userId, request.Devicetoken).ConfigureAwait(false);
                            }
                            else
                            {
                                await _userRepository.DeleteDeviceToken(userId, devicetoken).ConfigureAwait(false);
                                await _userRepository.InsertDeviceToken(userId, request.Devicetoken).ConfigureAwait(false);
                            }
                        }

                        var user = await _userRepository.GetNewRegisterUserById(userId);
                        if (user.RoleId == (int)RoleMasterEnum.Customer || user.RoleId == (int)RoleMasterEnum.Temporary)
                        {
                            var AddUserToWallet = await _walletRepository.AddUserWallet(Convert.ToInt32(userId));
                        }
                        //mail send 
                        var company = await _companyRepository.GetCompanyDetails(request.CompanyId).ConfigureAwait(false);
                        //var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
                        var companyDetails = await _companyHelper.GetCompany(request.CompanyId).ConfigureAwait(false);
                       // var welcomeEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Welcome Email")); 
                        
                        var result = new LoginResult
                        {
                            UserId = userId,
                            EmailId = request.EmailId,
                            Mobile = request.MobileNumber,
                            Role = user.Role,
                            Message = "User registered successfully!",
                            IsUserValid = true,
                            IsProfileComplete = false,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            IsBusiness = user.IsBusiness
                        };
                        AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                        {
                            UserID = userId,
                            Title = "New user registered",
                            Description = " New user registered " + company.CompanyName,
                            IsRead = (int)StatusEnum.Sent,
                            NotificationType = (int)NotificationType.Register

                        };
                        await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                        string title = "Registered successfully";
                        string subtitle = " ";
                        string forEvent = "newUser";


                        var group = await _notificationRepository.GetGroups().ConfigureAwait(false);
                        var newUserGroup = group.FirstOrDefault(g => g.Name.Equals("New Users"));

                        List<int> userIds = new List<int>();
                        userIds.Add(userId);
                        var req = new SubscribeTopicsforUsersRequestQuery()
                        {
                            GroupId = newUserGroup.Id,
                            UserIds = userIds
                        };
                        await _notificationRepository.AddNewUserInCustomersInNotificationTopics(userId, newUserGroup.Id).ConfigureAwait(false);
                        //string htmlTemplate = welcomeEmail.Html;
                        //var matches = Regex.Matches(welcomeEmail.Html, @"{{(.*?)}}");
                        //List<string> placeholders = matches.Cast<Match>()
                        //                        .Select(m => m.Groups[1].Value) // Group[1] is the captured variable name
                        //                        .Distinct()
                        //                        .ToList();
                        //var userDict = _genericRepository.ToDictionary(user);
                        //foreach (var key in placeholders)
                        //{
                        //    if (userDict.TryGetValue(key, out var value))
                        //    {
                        //        welcomeEmail.Html = welcomeEmail.Html.Replace("{{" + key + "}}", user.FirstName);
                        //    }
                        //}
                        //if (!string.IsNullOrEmpty(welcomeEmail.Html))
                        //{
                        //    var model = new PropertyUserWelcomeEmailDto
                        //    {
                        //        CompanyName = companyDetails.Name,
                        //        FirstName = user.FirstName,
                        //        Email = user.Email,
                        //        Mobile = user.Mobile,
                        //        companyEmail = companyDetails.Email,
                        //        Domain = companyDetails.Domain,
                        //        companyLogo = companyDetails.RelativeUrl,
                                
                        //    };
                        //    var template = Template.Parse(welcomeEmail.Html);
                        //    welcomeEmail.Html = template.Render(model, memberRenamer: member => member.Name);
                        //}
                        EmailModelClass obj = new()
                        {
                            title = "Registered successfully",
                            email = user.Email,
                            forEvent = "newUser",
                            subtitle = "",
                            mobile = user.Mobile,
                            propertyUser = user.FirstName,
                            body = "You are successfully registered into " + company.CompanyName + " . Kindly update your profile to proceed.",
                           // body=welcomeEmail.Html,
                            documentPath = "",
                            companyId = user.CompanyId

                        };
                       
                        await _otpService.SendEventMail(obj).ConfigureAwait(false);

                        return result;
                    }
                    else
                    {
                        validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                        {
                            PropertyName = nameof(RegisterUserCommand.Otp),
                            ErrorMessage = "Incorrect OTP."
                        });
                        if (!validatorResult.IsValid)
                            throw new ValidationException(validatorResult.Errors);
                    }
                }
                else
                {
                    GetUserByEmailComapnyId requestUser = new()
                    {
                        CompanyId = request.CompanyId,
                        Email = request.EmailId,
                    };
                    var result = new LoginResult();
                    var user = await _userRepository.GetUserByEmailComapnyId(requestUser).ConfigureAwait(false);
                    if (otpModel != null && user != null)
                    {
                        await _otpRepository.UpdateVerifiedOtp(otpModel.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);
                        validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                        {
                            PropertyName = nameof(RegisterUserCommand.Otp),
                            ErrorMessage = "User already exist, please sign in."
                        });
                        if (!validatorResult.IsValid)
                            throw new ValidationException(validatorResult.Errors);
                    }

                    return result;

                }
            }

            return new LoginResult();
        }

        #region ResetPassword
        public async Task<AddUpdateResultDto> Handle(ResetPassword request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new ResetPasswordValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var response = new AddUpdateResultDto();
            var getUserByEmailMobile = new GetUserByEmailComapnyId
            {
                CompanyId = request.CompanyId,
                Email = request.EmailMobile
            };
            var mobile = "";
            var email = "";

            if (request.EmailMobile.Length == 10 && !request.EmailMobile.Contains(".com"))
            {
                mobile = request.EmailMobile;
            }
            if (request.EmailMobile.Length != 10 && request.EmailMobile.Contains(".com"))
            {
                email = request.EmailMobile;
            }
            
            var user = await _userRepository.GetUserByEmailComapnyId(getUserByEmailMobile).ConfigureAwait(false);
            var commId = await _communicationRepository.GetCommunicationsByUserId(user.Id);
            if (user != null)
            {
                request.Password = _encryptionandDecryption.Encrypt(request.Password);
                request.CountryCodeId = await _otpRepository.GetCountryCodeId(request.CountryCode, 1).ConfigureAwait(false);
                var verifiedKey = await _otpRepository.GetVerifiedKey(mobile, user.CompanyId, email).ConfigureAwait(false);
                if (user.Password == request.Password)
                {
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(ResetPassword.Password),
                        ErrorMessage = " The old and new password must be different."
                    });
                }

                if (verifiedKey != request.VerifiedKey)
                {
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(ResetPassword.EmailMobile),
                        ErrorMessage = " You are not athorized."
                    });
                }
                else
                {
                    var result = await _userRepository.UpdatePassword(request.Password, user.Id);
                    response.Id = result;
                    response.Message = " Password reset successfully!";
                    string body = "Password reset for user name : " + user.UserName;
                    AddOrUpdateNotificationsQuery newNotification = new()
                    {
                        UserID = user.Id,
                        Title = "Password Reset successfully",
                        Description = body,
                        IsRead = (int)StatusEnum.Sent,
                        NotificationType = (int)NotificationType.Updated

                    };
                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

                    //var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
                    // var passwordEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Password Reset"));
                    //if (!string.IsNullOrEmpty(passwordEmail.Html))
                    //{
                    //    string htmlTemplate = passwordEmail.Html;
                    //    var matches = Regex.Matches(passwordEmail.Html, @"{{(.*?)}}");
                    //    List<string> placeholders = matches.Cast<Match>()
                    //                            .Select(m => m.Groups[1].Value) // Group[1] is the captured variable name
                    //    .Distinct()
                    //                            .ToList();
                    //    var userDict = _genericRepository.ToDictionary(user);
                    //    foreach (var key in placeholders)
                    //    {
                    //        if (userDict.TryGetValue(key, out var value))
                    //        {
                    //            passwordEmail.Html = passwordEmail.Html.Replace("{{" + key + "}}", user.FirstName);
                    //        }
                    //    }
                    string title = "Password Reset successfully";
                    EmailModelClass obj = new()
                        {

                            title = title,
                            email = user.Email,
                            forEvent = "PasswordChanged",
                            subtitle = "",
                            mobile = user.Mobile,
                            propertyUser = user.UserName,
                            body = body,
                            documentPath = "",
                            companyId = user.CompanyId
                        };
                        if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
                        {
                            await _otpService.SendEventMail(obj).ConfigureAwait(false);
                        }
                    // }
                    
                    await _userRepository.LogOutUsersAllSessions(result).ConfigureAwait(false);
                }
            }
            else
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(ResetPassword.EmailMobile),
                    ErrorMessage = " Invalid email or mobile."
                });
            }

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return response;
        }


        #endregion

        public async Task<string> Handle(LogOutRequestQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new LogOutRequestQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            await _userRepository.LogOutUsersSessions(request.UserId, request.SessionKey).ConfigureAwait(false);
            await _userRepository.LogOutUsersDeivcetoken(request.UserId).ConfigureAwait(false);

            return "Singed out successfully!";

        }
        
    }

}
