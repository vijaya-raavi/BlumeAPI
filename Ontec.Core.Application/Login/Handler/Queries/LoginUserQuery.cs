using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Requests.Login.Queries;

namespace Ontec.Core.Application.Login.Handler.Queries
{
    public class LoginUserQuery : IRequestHandler<GetUserByEmailQuery, LoginResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEncryptionandDecryption _encryptionandDecryption;
        private readonly IOtpService _otpService;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IPushNotification _pushNotification;
        private readonly IUserBlockService _userBlockService;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IGenericRepository _genericRepository;
        public LoginUserQuery(IUserRepository userRepository,
                              IEncryptionandDecryption encryptionandDecryption,
                              IOtpService otpService,
                              ICommunicationRepository communicationRepository,
                              IPushNotification pushNotification,
                              IUserBlockService userBlockService,
                              IEmailTemplateRepository emailTemplateRepository,
                              IGenericRepository genericRepository)
        {
            _userRepository = userRepository;
            _encryptionandDecryption = encryptionandDecryption;
            _otpService = otpService;
            _communicationRepository = communicationRepository;
            _pushNotification = pushNotification;
            _userBlockService = userBlockService;
            _emailTemplateRepository = emailTemplateRepository;
            _genericRepository = genericRepository;


        }
        //public async Task<LoginResult> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        //{

        //    request.TrimAllStrings();
        //    var logResult = new LoginResult();
        //    var commonValidator = new GetUserByEmailQueryValidator(_userRepository, _encryptionandDecryption, _otpService, _communicationRepository, _userBlockService);
        //    var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
        //    if (!validatorResult.IsValid)
        //        throw new ValidationException(validatorResult.Errors);

        //    var encryptedPassword = _encryptionandDecryption.Encrypt(request.Password);
        //    request.Password = encryptedPassword;
        //    var data = await _userRepository.IsUserExist(request).ConfigureAwait(false);

        //    string title;
        //    string subtitle = "";
        //    string forEvent = "Deregister";
        //    var useStatus = StatusEnum.Pending;
        //    if (data != null && data.Status == useStatus.ToString())
        //    {
        //        validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
        //        {
        //            PropertyName = nameof(GetUserByEmailQuery.Email),
        //            ErrorMessage = "Your registration request is sent to admin for approval."
        //        });
        //    }
        //    if (!validatorResult.IsValid)
        //        throw new ValidationException(validatorResult.Errors);

        //    if (data != null)
        //    {
        //        string devicetoken = "";
        //        var user = await _userRepository.GetUserById(data.UserId).ConfigureAwait(false);


        //        var commId = await _communicationRepository.GetCommunicationsByUserId(user.Id);
        //        if (user.RoleId == (int)RoleMasterEnum.Customer)
        //            data.IsProfileComplete = (user.CommunicationTypesIds.Any() || user.StatusId == (int)StatusEnum.Rejected);
        //        else
        //            data.IsProfileComplete = true;
        //        data.IsEstateEnable = user.IsEstateEnable;
        //        data.Auxaccountdetails = user.Auxaccountdetails;

        //        if (data != null && string.IsNullOrEmpty(data.EmailId) && !data.IsBlocked)
        //        {
        //            var message = "Email or password is incorrect";
        //            if (request.IsAdmin)
        //            {
        //                message += " or you are not authorized";
        //            }
        //            message += ".";
        //            validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
        //            {
        //                PropertyName = nameof(GetUserByEmailQuery.Email),
        //                ErrorMessage = message
        //            });
        //        }
        //        else if (data != null && data.IsBlocked && data.FailedCountAttempted == 5)
        //        {
        //            validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
        //            {
        //                PropertyName = nameof(GetUserByEmailQuery.Email),
        //                ErrorMessage = "Your account has been locked for 20 minutes, please contact to admin."
        //            });
        //        }

        //        if (user.LastLoginDate.HasValue)
        //        {
        //            try
        //            {
        //                DateTime lastlogindate = user.LastLoginDate.Value;
        //                DateTime currentdate = Convert.ToDateTime(DateTime.UtcNow);
        //                TimeSpan objTimeSpan = currentdate - lastlogindate;
        //                double Days = Convert.ToDouble(objTimeSpan.TotalDays);
        //                int loginYearDays = DateTime.IsLeapYear(lastlogindate.Year) ? 366 : 365;
        //                int currentYearDays = DateTime.IsLeapYear(currentdate.Year) ? 366 : 365;

        //                int months = ((currentdate.Year - lastlogindate.Year) * 12) + currentdate.Month - lastlogindate.Month;
        //                EmailModelClass obj1 = new()
        //                {
        //                    title = "Not logged in to system",
        //                    email = user.Email,
        //                    forEvent = "Deregister",
        //                    subtitle = "",
        //                    mobile = "",
        //                    propertyUser = user.UserName,
        //                    body = "",
        //                    documentPath = "",
        //                    companyId = request.CompanyId
        //                };

        //                if (currentdate > lastlogindate.AddYears(1))
        //                {
        //                    if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
        //                    {
        //                        await _otpService.SendEventMail(obj1).ConfigureAwait(false);
        //                    }
        //                    await _userRepository.DeActiveUserById(user.Id).ConfigureAwait(false);
        //                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
        //                    {
        //                        PropertyName = nameof(GetUserByEmailQuery.Email),
        //                        ErrorMessage = "Your account has been deactivated due to prolonged inactivity.Please contact support to reactivate your account."
        //                    });
        //                    if (!validatorResult.IsValid)
        //                        throw new ValidationException(validatorResult.Errors);
        //                }
        //                else
        //                {
        //                    EmailModelClass obj = new()
        //                    {

        //                        title = "Signed in successfully",
        //                        email = user.Email,
        //                        forEvent = "SuccessFullLogin",
        //                        subtitle = "",
        //                        mobile = user.Mobile,
        //                        propertyUser = user.UserName,
        //                        body = "",
        //                        documentPath = "",
        //                        companyId = request.CompanyId
        //                    };
        //                    if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
        //                    {
        //                        await _otpService.SendEventMail(obj).ConfigureAwait(false);
        //                    }
        //                    await _userRepository.UpdateLoginAttempt(user.Id, 0, false).ConfigureAwait(false);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //        }


        //        //var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
        //        //var loginEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Login Alert Email"));
        //        //if (!string.IsNullOrEmpty(loginEmail.Html))
        //        //{
        //        //    string htmlTemplate = loginEmail.Html;
        //        //    var matches = Regex.Matches(loginEmail.Html, @"{{(.*?)}}");
        //        //    List<string> placeholders = matches.Cast<Match>()
        //        //                            .Select(m => m.Groups[1].Value) // Group[1] is the captured variable name
        //        //    .Distinct()
        //        //                            .ToList();
        //        //    var userDict = _genericRepository.ToDictionary(user);
        //        //    foreach (var key in placeholders)
        //        //    {
        //        //        if (userDict.TryGetValue(key, out var value))
        //        //        {
        //        //            loginEmail.Html = loginEmail.Html.Replace("{{" + key + "}}", user.FirstName);
        //        //        }
        //        //    }





        //        if (data != null && !string.IsNullOrEmpty(request.Devicetoken) && request.Devicetoken != "string")
        //        {

        //            devicetoken = await _userRepository.GetDeviceToken(data.UserId).ConfigureAwait(false);
        //            if (devicetoken == "string")
        //            {
        //                await _userRepository.DeleteDeviceToken(data.UserId, devicetoken).ConfigureAwait(false);
        //            }
        //            if (string.IsNullOrEmpty(devicetoken))
        //            {
        //                await _userRepository.InsertDeviceToken(data.UserId, request.Devicetoken).ConfigureAwait(false);
        //            }
        //            else
        //            {
        //                await _userRepository.DeleteDeviceToken(data.UserId, devicetoken).ConfigureAwait(false);
        //                await _userRepository.InsertDeviceToken(data.UserId, request.Devicetoken).ConfigureAwait(false);
        //            }

        //        }


        //        data.IsBlocked = false;
        //    }


        //    if (!validatorResult.IsValid)
        //        throw new ValidationException(validatorResult.Errors);

        //    return data;
        //}


        public async Task<LoginResult> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {

            request.TrimAllStrings();
            var logResult = new LoginResult();
            var commonValidator = new GetUserByEmailQueryValidator(_userRepository, _encryptionandDecryption, _otpService, _communicationRepository, _userBlockService);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var encryptedPassword = _encryptionandDecryption.Encrypt(request.Password);
            request.Password = encryptedPassword;
            var data = await _userRepository.IsUserExist(request).ConfigureAwait(false);

            string title;
            string subtitle = "";
            string forEvent = "Deregister";
            var useStatus = StatusEnum.Pending;
            if (data != null && data.Status == useStatus.ToString())
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(GetUserByEmailQuery.Email),
                    ErrorMessage = "Your registration request is sent to admin for approval."
                });
            }
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            if (data != null)
            {
                string devicetoken = "";
                var user = await _userRepository.GetUserById(data.UserId).ConfigureAwait(false);


                var commId = await _communicationRepository.GetCommunicationsByUserId(user.Id);
                if (user.RoleId == (int)RoleMasterEnum.Customer)
                    data.IsProfileComplete = (user.CommunicationTypesIds.Any() || user.StatusId == (int)StatusEnum.Rejected);
                else
                    data.IsProfileComplete = true;
                data.IsEstateEnable = user.IsEstateEnable;

                if (data != null && string.IsNullOrEmpty(data.EmailId) && !data.IsBlocked)
                {
                    var message = "Email or password is incorrect";
                    if (request.IsAdmin)
                    {
                        message += " or you are not authorized";
                    }
                    message += ".";
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(GetUserByEmailQuery.Email),
                        ErrorMessage = message
                    });
                }
                else if (data != null && data.IsBlocked && data.FailedCountAttempted == 5)
                {
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(GetUserByEmailQuery.Email),
                        ErrorMessage = "Your account has been locked for 20 minutes, please contact to admin."
                    });
                }
                if (user.LastLoginDate.HasValue)
                {
                    try
                    {
                        DateTime lastlogindate = user.LastLoginDate.Value;
                        DateTime currentdate = Convert.ToDateTime(DateTime.UtcNow);
                        TimeSpan objTimeSpan = currentdate - lastlogindate;
                        double Days = Convert.ToDouble(objTimeSpan.TotalDays);
                        int loginYearDays = DateTime.IsLeapYear(lastlogindate.Year) ? 366 : 365;
                        int currentYearDays = DateTime.IsLeapYear(currentdate.Year) ? 366 : 365;

                        int months = ((currentdate.Year - lastlogindate.Year) * 12) + currentdate.Month - lastlogindate.Month;
                        EmailModelClass obj1 = new()
                        {
                            title = "Not logged in to system",
                            email = user.Email,
                            forEvent = "Deregister",
                            subtitle = "",
                            mobile = "",
                            propertyUser = user.UserName,
                            body = "",
                            documentPath = "",
                            companyId = request.CompanyId
                        };

                        if (currentdate > lastlogindate.AddYears(1))
                        {
                            if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
                            {
                                await _otpService.SendEventMail(obj1).ConfigureAwait(false);
                            }
                            await _userRepository.DeActiveUserById(user.Id).ConfigureAwait(false);
                            validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                            {
                                PropertyName = nameof(GetUserByEmailQuery.Email),
                                ErrorMessage = "Your account has been deactivated due to prolonged inactivity.Please contact support to reactivate your account."
                            });
                            if (!validatorResult.IsValid)
                                throw new ValidationException(validatorResult.Errors);
                        }
                        else
                        {
                            EmailModelClass obj = new()
                            {

                                title = "Signed in successfully",
                                email = user.Email,
                                forEvent = "SuccessFullLogin",
                                subtitle = "",
                                mobile = user.Mobile,
                                propertyUser = user.UserName,
                                body = "",
                                documentPath = "",
                                companyId = request.CompanyId
                            };
                            if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
                            {
                                await _otpService.SendEventMail(obj).ConfigureAwait(false);
                            }
                            await _userRepository.UpdateLoginAttempt(user.Id, 0, false).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }


                if (data != null && !string.IsNullOrEmpty(request.Devicetoken) && request.Devicetoken != "string")
                {

                    devicetoken = await _userRepository.GetDeviceToken(data.UserId).ConfigureAwait(false);
                    if (devicetoken == "string")
                    {
                        await _userRepository.DeleteDeviceToken(data.UserId, devicetoken).ConfigureAwait(false);
                    }
                    if (string.IsNullOrEmpty(devicetoken))
                    {
                        await _userRepository.InsertDeviceToken(data.UserId, request.Devicetoken).ConfigureAwait(false);
                    }
                    else
                    {
                        await _userRepository.DeleteDeviceToken(data.UserId, devicetoken).ConfigureAwait(false);
                        await _userRepository.InsertDeviceToken(data.UserId, request.Devicetoken).ConfigureAwait(false);
                    }

                }


                data.IsBlocked = false;
            }


            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return data;
        }
    }
}
