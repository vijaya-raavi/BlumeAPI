using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Otp;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Otp.Command;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Application.Otp.Handler.Command
{
    public class OtpCommandHandler : IRequestHandler<VerifyForgotPasswordOtpQuery, VerifyOTPDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpRepository _otpRepository;
        public OtpCommandHandler(IUserRepository userRepository
                                , IOtpRepository otpRepository)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
        }
        public async Task<VerifyOTPDto> Handle(VerifyForgotPasswordOtpQuery request, CancellationToken cancellationToken)
        {
            var res = new VerifyOTPDto();
            var commonValidator = new VerifyForgotPasswordOtpQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var getUserByEmailMobile = new GetUserByEmailComapnyId
            {
                CompanyId = request.CompanyId,
                Email = request.EmailMobile
            };

            var user = await _userRepository.GetUserByEmailComapnyId(getUserByEmailMobile).ConfigureAwait(false);
            if (user != null)
            {
                var otpModel = await _otpRepository.GetOtpByMobileNumberCompanyId(user.Mobile, request.CompanyId, user.Email, (int)StatusEnum.NotVerified).ConfigureAwait(false);
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
                TimeSpan span = nowTime.Subtract(otpModel.CreatedAt.ToUniversalTime());
                int otpDuration = span.Minutes;
                if (otpDuration > 10)
                {
                    await _otpRepository.UpdateVerifiedOtp(otpModel.Id, (int)StatusEnum.Inactive,null).ConfigureAwait(false);
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(RegisterUserCommand.Otp),
                        ErrorMessage = "Your OTP is expired."
                    });
                    if (!validatorResult.IsValid)
                        throw new ValidationException(validatorResult.Errors);
                }

                else if (otpModel != null && otpModel.Otp.Trim() == request.OTP)
                {
                    var verifiedKey = ChecksumHelper.GenerateVerifiedKey();
                    await _otpRepository.UpdateVerifiedOtp(otpModel.Id, (int)StatusEnum.Pending,verifiedKey).ConfigureAwait(false);
                    res.Message= "OTP verified successfully!";
                    res.VerifiedKey = verifiedKey;
                }
                else if (otpModel != null && otpModel.Otp.Trim() != request.OTP)
                {
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(VerifyForgotPasswordOtpQuery.OTP),
                        ErrorMessage = "Incorrect OTP."
                    });
                }

            }
            else
            {

                validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = nameof(VerifyForgotPasswordOtpQuery.OTP),
                    ErrorMessage = "Incorrect OTP."
                });
            }

           
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return res;

        }
    }
}
