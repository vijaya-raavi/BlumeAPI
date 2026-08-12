using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Domain.Requests.BiometricVerification.Command
{
    public class RegisterBiometricRequestValidator:AbstractValidator<RegisterBiometricRequest>
    {
        public RegisterBiometricRequestValidator(IUserRepository _userRepository,IWorkContext workContext)
        {
            RuleFor(x => x.UserId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetUserByIdQuery.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isxist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isxist)
                {
                    context.AddFailure(nameof(RegisterBiometricRequest.UserId), string.Format(CommonConstants.NotExist, nameof(RegisterBiometricRequest.UserId)));
                }

                if (isxist && workContext.CurrentUserId != model.UserId)
                {
                    context.AddFailure(nameof(RegisterBiometricRequest.UserId), string.Format(CommonConstants.Unauthorized, nameof(RegisterBiometricRequest.UserId)));
                }
            });
        }
    }
}
