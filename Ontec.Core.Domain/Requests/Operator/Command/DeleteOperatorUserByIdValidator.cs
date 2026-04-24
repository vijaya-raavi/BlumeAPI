using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Operator.Command
{
    public class DeleteOperatorUserByIdValidator : AbstractValidator<DeleteOperatorUserById>
    {
        public DeleteOperatorUserByIdValidator(IUserRepository _userRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(DeleteOperatorUserById.Id), string.Format(CommonConstants.NotExist, nameof(DeleteOperatorUserById.Id)));
            });
        }
    }
}
