using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class VerifyUserQueryRequestValidator : AbstractValidator<VerifyUserQueryRequest>
    {
        public VerifyUserQueryRequestValidator(IUserRepository _userRepository,IWorkContext _workContext) 
        {

            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdNotInprocess(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(VerifyUserQueryRequest.UserId), string.Format(CommonConstants.NotExist, nameof(VerifyUserQueryRequest.UserId)));
               

            });
        }
    }
}
