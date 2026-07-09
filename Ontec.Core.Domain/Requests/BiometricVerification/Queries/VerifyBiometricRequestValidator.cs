using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Domain.Requests.BiometricVerification.Queries
{
    public  class VerifyBiometricRequestValidator:AbstractValidator<VerifyBiometricRequest>
    {
        public VerifyBiometricRequestValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.UserId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetUserByIdQuery.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isxist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isxist)
                {
                    context.AddFailure(nameof(VerifyBiometricRequest.UserId), string.Format(CommonConstants.NotExist, nameof(VerifyBiometricRequest.UserId)));
                }
                int publicKeyId = await _userRepository.IsBiometricExist(model.UserId, model.DeviceId).ConfigureAwait(false);

                if (publicKeyId == 0)
                {
                    context.AddFailure(nameof(VerifyBiometricRequest.UserId), string.Format(CommonConstants.BiometricNotRegistered, nameof(VerifyBiometricRequest.UserId)));
                }
            });
        }
    }
}
