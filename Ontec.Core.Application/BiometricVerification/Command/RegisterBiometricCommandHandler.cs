using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Requests.BiometricVerification.Command;
using Ontec.Core.Domain.Requests.BiometricVerification.Queries;
using Ontec.Core.Domain.Requests.Login.Queries;
using Org.BouncyCastle.Crypto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ontec.Core.Application.BiometricVerification.Command
{
    public class RegisterBiometricCommandHandler : IRequestHandler<RegisterBiometricRequest, AddUpdateResultDto>,
                                                    IRequestHandler<VerifyBiometricRequest, LoginResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkContext _workContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IGenericRepository _genericRepository;
        private readonly IOtpService _otpService;
        private readonly ICommunicationRepository _communicationRepository;
        public RegisterBiometricCommandHandler(IUserRepository userRepository,
            IWorkContext workContext,
            IMemoryCache memoryCache,
             IEmailTemplateRepository emailTemplateRepository,
             ICommunicationRepository communicationRepository,
             IGenericRepository genericRepository,
             IOtpService otpService)
        {
            _userRepository = userRepository;
            _memoryCache = memoryCache;
            _workContext = workContext;
            _emailTemplateRepository = emailTemplateRepository;
            _communicationRepository = communicationRepository;
            _genericRepository = genericRepository;
            _otpService = otpService;
        }

        public async Task<AddUpdateResultDto> Handle(RegisterBiometricRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new RegisterBiometricRequestValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var response = new AddUpdateResultDto();
            var id = await _userRepository.IsBiometricExist(request.UserId, request.DeviceId).ConfigureAwait(false);
            if (id == 0)
            {
                response.Id = await _userRepository.RegisterBiometric(request).ConfigureAwait(false);
            }
            else
            {
                response.Id = await _userRepository.UpdateBiometric(id, request.PublicKey).ConfigureAwait(false);
            }
            if (response.Id > 0)
            {
                response.Message = "Biometric registered successfully";
            }
            return response;
        }
        public async Task<LoginResult> Handle(VerifyBiometricRequest request, CancellationToken cancellationToken)
        {
            bool isValid = false;
            var data = new LoginResult();
            request.TrimAllStrings();
            var commonValidator = new VerifyBiometricRequestValidator(_userRepository);

            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            string title;
            string subtitle = "";
            string forEvent = "Deregister";
            var useStatus = StatusEnum.Pending;
            // Validate challenge
            var cacheKey = $"BIO_{request.DeviceId}_{request.UserId}";


            var savedChallenge = _memoryCache.Get<string>(cacheKey);


            if (savedChallenge != request.Challenge)
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(GetUserByEmailQuery.Email),
                    ErrorMessage = "Invalid challange."
                });
            }

            // Get biometric data
            var biometric = await _userRepository.GetUserBiometricByDeviceIdUserId(request.DeviceId, request.UserId).ConfigureAwait(false);

            if (biometric == null)
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(GetUserByEmailQuery.Email),
                    ErrorMessage = "Device not registered."
                });
            }
            else
            {
                // Verify signature
                isValid = VerifySignature(biometric.PublicKey, request.Challenge, request.Signature,request.Platform);
            }
            if (!isValid)
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(GetUserByEmailQuery.Email),
                    ErrorMessage = "Invalid biometric signature."
                });
            }

            // Generate JWT

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            _memoryCache.Remove(cacheKey);

            var user = await _userRepository.GetUserById(request.UserId).ConfigureAwait(false);
            // return user
            var getLoginUserRequest = new GetUserByEmailQuery()
            {
                CompanyId = user.CompanyId,
                Email = user.Email
            };


            data = await _userRepository.IsBiometricUserExist(getLoginUserRequest).ConfigureAwait(false);
            if (data != null)
            {
                var commId = await _communicationRepository.GetCommunicationsByUserId(user.Id);
                if (user.RoleId == (int)RoleMasterEnum.Customer)
                    data.IsProfileComplete = (user.CommunicationTypesIds.Any() || user.StatusId == (int)StatusEnum.Rejected);
                else
                    data.IsProfileComplete = true;


                if (user.LastLoginDate.HasValue)
                {
                    try
                    {
                        DateTime lastlogindate = user.LastLoginDate.Value;
                        DateTime currentdate = Convert.ToDateTime(DateTime.UtcNow);
                        TimeSpan objTimeSpan = currentdate - lastlogindate;
                        double Days = Convert.ToDouble(objTimeSpan.TotalDays);
                        DateTime twelveMonthsAgo = currentdate.AddMonths(-12);
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
                            companyId = data.CompanyId
                        };

                        //if (Days >= 172)
                        //{
                        if (lastlogindate < twelveMonthsAgo)
                        {
                            if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
                            {
                                await _otpService.SendEventMail(obj1).ConfigureAwait(false);
                            }
                            await _userRepository.DeActiveUserById(user.Id).ConfigureAwait(false);
                            validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                            {
                                PropertyName = nameof(GetUserByEmailQuery.Email),
                                ErrorMessage = "Your account is deregister."
                            });
                            if (!validatorResult.IsValid)
                                throw new ValidationException(validatorResult.Errors);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }


                var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
                var loginEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Login Alert Email"));
                if (!string.IsNullOrEmpty(loginEmail?.Html))
                {
                    string htmlTemplate = loginEmail.Html;
                    var matches = Regex.Matches(loginEmail.Html, @"{{(.*?)}}");
                    List<string> placeholders = matches.Cast<Match>()
                                            .Select(m => m.Groups[1].Value) // Group[1] is the captured variable name
                    .Distinct()
                                            .ToList();
                    var userDict = _genericRepository.ToDictionary(user);
                    foreach (var key in placeholders)
                    {
                        if (userDict.TryGetValue(key, out var value))
                        {
                            loginEmail.Html = loginEmail.Html.Replace("{{" + key + "}}", user.FirstName);
                        }
                    }
                    EmailModelClass obj = new()
                    {

                        title = "Signed in successfully",
                        email = user.Email,
                        forEvent = "SuccessFullLogin",
                        subtitle = "",
                        mobile = user.Mobile,
                        propertyUser = user.UserName,
                        body = loginEmail.Html,
                        documentPath = "",
                        companyId = data.CompanyId
                    };
                    if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
                    {
                        await _otpService.SendEventMail(obj).ConfigureAwait(false);
                    }
                }
                await _userRepository.UpdateLastLoginDate(user.Id).ConfigureAwait(false);

                data.IsBlocked = false;
            }

            return data;

        }

        //private bool VerifySignature(string publicKey, string challenge, string signature)
        //{
        //    bool isValid = false;
        //    try
        //    {
        //        using RSA rsa = RSA.Create();
        //        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
        //        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
        //        var challengeBytes = Encoding.UTF8.GetBytes(challenge);
        //        var signatureBytes = Convert.FromBase64String(signature);

        //        isValid = rsa.VerifyData(challengeBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        //        if (!isValid)
        //        {
        //            isValid = rsa.VerifyData(challengeBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        //        }
        //        return isValid;
        //    }
        //    catch
        //    {
        //        return isValid;
        //    }
        //}

        private bool VerifySignature(string publicKey, string challenge, string signature, string platform = "unknown")
        {
            try
            {
                byte[] keyBytes = Convert.FromBase64String(CleanPublicKey(publicKey));
                byte[] challengeBytes = Encoding.UTF8.GetBytes(challenge);
                byte[] signatureBytes = Convert.FromBase64String(signature);


                using RSA rsa = RSA.Create();
                ImportKey(rsa, keyBytes, platform);

                // Try SHA256 + PKCS1 (most common for both iOS and Android)
                if (TryVerify(rsa, challengeBytes, signatureBytes,HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                {
                    return true;
                }

                // Try SHA256 + PSS
                if (TryVerify(rsa, challengeBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
                {
                    return true;
                }

                // Try SHA1 + PKCS1 (older Android devices)
                if (TryVerify(rsa, challengeBytes, signatureBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VerifySignature] ❌ Exception: {ex.Message}");
                return false;
            }
        }




        private void ImportKey(RSA rsa, byte[] keyBytes, string platform)
        {
            if (platform?.ToLower() == "android")
            {
                rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                return;
            }

            if (platform?.ToLower() == "ios")
            {
                try
                {
                    rsa.ImportRSAPublicKey(keyBytes, out _);
                }
                catch
                {
                    rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                }
                return;
            }
            try
            {
                if (keyBytes[0] == 0x30)
                {
                    rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                    Console.WriteLine("[ImportKey] Auto: PKCS#8");
                }
                else
                {
                    rsa.ImportRSAPublicKey(keyBytes, out _);
                    Console.WriteLine("[ImportKey] Auto: PKCS#1");
                }
            }
            catch
            {
                try
                {
                    rsa.ImportRSAPublicKey(keyBytes, out _);
                }
                catch
                {
                    rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
                }
            }
        }

        private bool TryVerify(RSA rsa, byte[] data, byte[] signature, HashAlgorithmName hash, RSASignaturePadding padding)
        {
            try
            {
                return rsa.VerifyData(data, signature, hash, padding);
            }
            catch
            {
                return false;
            }
        }

        private string CleanPublicKey(string key) => key
            .Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("-----BEGIN RSA PUBLIC KEY-----", "")
            .Replace("-----END RSA PUBLIC KEY-----", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace(" ", "")
            .Trim();
    }
}
