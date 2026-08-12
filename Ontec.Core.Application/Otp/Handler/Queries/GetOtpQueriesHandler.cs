using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto.Otp;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Login.Queries;
using Ontec.Core.Domain.Requests.User.Queries;
using static System.Net.WebRequestMethods;

namespace Ontec.Core.Application.Otp.Handler.Queries
{
    public class GetOtpQueriesHandler : IRequestHandler<GetRegisterOtpQuery, OtpResponseModel>
                                        , IRequestHandler<GetForgotPasswordOtpQuery, OtpResponseModel>
                                         , IRequestHandler<GetUpdationOtpQuery, OtpResponseModel>
    {
        private readonly IOtpService _otpService;
        private readonly IOtpRepository _otpRepository;
        private readonly IWorkContext _workContext;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        public GetOtpQueriesHandler(IOtpService otpService
                                    , IOtpRepository otpRepository
                                    , IWorkContext workContext
                                    , IUserRepository userRepository
                                    , ICompanyRepository companyRepository
                                )
        {
            _otpService = otpService;
            _otpRepository = otpRepository;
            _workContext = workContext;
            _userRepository = userRepository;
            _companyRepository = companyRepository;

        }
        public async Task<OtpResponseModel> Handle(GetRegisterOtpQuery request, CancellationToken cancellationToken)
        {
            var commonValidator = new GetRegisterOtpQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var res = new OtpResponseModel();
            Random generator = new Random();
            //var r = "123456";
             var r = ChecksumHelper.GenerateOTP();
            var otpModel = new OtpModel
            {
                CompanyId = request.CompanyId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Email = request.Email,
                MobileNumber = request.MobileNumber,
                Otp = r,
                StatusId = (int)StatusEnum.NotVerified,
                CountryCodeId = await _otpRepository.GetCountryCodeId(request.CountryCode, (int)StatusEnum.Active).ConfigureAwait(false)
            };

            var existingOtp = await _otpRepository.GetOtpByMobileNumberCompanyId(request.MobileNumber, request.CompanyId, request.Email, (int)StatusEnum.NotVerified).ConfigureAwait(false);
            if (existingOtp != null)
            {
                _ = await _otpRepository.UpdateVerifiedOtp(existingOtp.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);
            }
            _ = await _otpRepository.InsertOtp(otpModel).ConfigureAwait(false);
            res.Type = "Register";
            res.Otp = r;

            _workContext.SetCurrentOtp(res);
            string body = " Your registration otp is : " + r;
            res.Otp = "";

            EmailModelClass obj = new()
            {
                title = "Notification for registration OTP",
                email = request.Email,
                forEvent = "SigUpOTP",
                subtitle = "",
                companyId = request.CompanyId,
                mobile = "",
                propertyUser = "",
                body = r,
                documentPath = ""

            };

            res.EmailResponse = await _otpService.SendEventMail(obj).ConfigureAwait(false);
            if (request.MobileNumber.Length == 10)
            {
                request.MobileNumber = request.MobileNumber.TrimStart('0');

            }
            res.MobileResponse = await _otpService.SendMobileOtp(r, body, request.CountryCode + request.MobileNumber).ConfigureAwait(false);

            return res;
        }

        public async Task<OtpResponseModel> Handle(GetForgotPasswordOtpQuery request, CancellationToken cancellationToken)
        {
            var commonValidator = new GetForgotPasswordOtpQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var res = new OtpResponseModel();
            Random generator = new Random();

             var r = ChecksumHelper.GenerateOTP();
            //var r = "123456";
            var getUserByEmailMobile = new GetUserByEmailComapnyId
            {
                CompanyId = request.CompanyId,
                Email = request.EmailMobile
            };
            var getUser = await _userRepository.GetUserByEmailComapnyId(getUserByEmailMobile).ConfigureAwait(false);
            if (getUser != null)
            {
                var otpModel = new OtpModel
                {
                    CompanyId = request.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Email = getUser.Email,
                    MobileNumber = getUser.Mobile,
                    Otp = r,
                    StatusId = (int)StatusEnum.NotVerified,
                    CountryCodeId = await _otpRepository.GetCountryCodeId(request.CountryCode, (int)StatusEnum.Active).ConfigureAwait(false)
                };
                var existingOtp = await _otpRepository.GetOtpByMobileNumberCompanyId(getUser.Mobile, request.CompanyId, getUser.Email, (int)StatusEnum.NotVerified).ConfigureAwait(false);
                if (existingOtp != null)
                {
                    _ = await _otpRepository.UpdateVerifiedOtp(existingOtp.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);
                }
                _ = await _otpRepository.InsertOtp(otpModel).ConfigureAwait(false);
                res.Type = "Forgot";
                res.Otp = r;
                _workContext.SetCurrentOtp(res);

                res.Otp = "";

                EmailModelClass obj = new()
                {

                    title = "OTP for reset your password",
                    email = getUser.Email,
                    forEvent = "ResetPassword",
                    subtitle = "",
                    companyId = request.CompanyId,
                    mobile = getUser.Mobile,
                    propertyUser = getUser.FirstName,
                    body = r,
                    documentPath = ""

                };

                res.EmailResponse = await _otpService.SendEventMail(obj).ConfigureAwait(false);
                string body = "Your OTP to reset password is : ";
                if (getUser.Mobile.Length == 10)
                {
                    getUser.Mobile = getUser.Mobile.TrimStart('0');

                }
                res.MobileResponse = await _otpService.SendMobileOtp(r, body + r, request.CountryCode + getUser.Mobile).ConfigureAwait(false);

            }
            else
            {
                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(ResetPassword.EmailMobile),
                    ErrorMessage = " Your does not exist in system."
                });
            }

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return res;
        }

        public async Task<OtpResponseModel> Handle(GetUpdationOtpQuery request, CancellationToken cancellationToken)
        {
            var commonValidator = new GetUpdationOtpQueryValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var res = new OtpResponseModel();
            Random generator = new Random();
            //var r = "123456";
            var getUser = new UserProfileDto();
              var r = ChecksumHelper.GenerateOTP();
            var mobile = "";
            var email = "";

            getUser = await _userRepository.GetUserById(request.UserId).ConfigureAwait(false);
            if (request.IsEmail)
            {

                var otpModel = new OtpModel
                {
                    CompanyId = request.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Email = request.EmailId,
                    MobileNumber = getUser.Mobile,
                    Otp = r,
                    StatusId = (int)StatusEnum.NotVerified,
                    CountryCodeId = await _otpRepository.GetCountryCodeId(request.CountryCode, (int)StatusEnum.Active).ConfigureAwait(false)
                };
                var existingOtp = await _otpRepository.GetOtpByMobileNumberCompanyId(mobile, request.CompanyId, request.EmailId, (int)StatusEnum.NotVerified).ConfigureAwait(false);
                if (existingOtp != null)
                {
                    _ = await _otpRepository.UpdateVerifiedOtp(existingOtp.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);
                }
                _ = await _otpRepository.InsertOtp(otpModel).ConfigureAwait(false);
            }
            else
            {
                var otpModel = new OtpModel
                {
                    CompanyId = request.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Email = getUser.Email,
                    MobileNumber = request.MobileNumber,
                    Otp = r,
                    StatusId = (int)StatusEnum.NotVerified,
                    CountryCodeId = await _otpRepository.GetCountryCodeId(request.CountryCode, (int)StatusEnum.Active).ConfigureAwait(false)
                };
                var existingOtp = await _otpRepository.GetOtpByMobileNumberCompanyId(request.MobileNumber, request.CompanyId,email, (int)StatusEnum.NotVerified).ConfigureAwait(false);
                if (existingOtp != null)
                {
                    _ = await _otpRepository.UpdateVerifiedOtp(existingOtp.Id, (int)StatusEnum.Inactive, null).ConfigureAwait(false);
                }
                _ = await _otpRepository.InsertOtp(otpModel).ConfigureAwait(false);

            }

           
            res.Type = "Update";
            res.Otp = r;

            _workContext.SetCurrentOtp(res);
            string body = r;
            res.Otp = "";

            if (request.IsEmail)
            {
                var getUserByEmailMobile = new GetUserByEmailComapnyId
                {
                    CompanyId = request.CompanyId,
                    Email = request.EmailId
                };

                EmailModelClass obj = new()
                {
                    title = "Notification for update contacts OTP",
                    email = request.EmailId,
                    forEvent = "UpdateContactsOTP",
                    subtitle = "",
                    companyId = request.CompanyId,
                    mobile = "",
                    propertyUser = getUser.FirstName,
                    body = r,
                    documentPath = ""

                };

                res.EmailResponse = await _otpService.SendEventMail(obj).ConfigureAwait(false);
            }
            else
            {
                if (request.MobileNumber.Length == 10)
                {
                    request.MobileNumber = request.MobileNumber.TrimStart('0');

                }
                res.MobileResponse = await _otpService.SendMobileOtp(r, "Your otp to update contact details is : "+r, request.CountryCode + request.MobileNumber).ConfigureAwait(false);
            }
            return res;
        }

    }
}
