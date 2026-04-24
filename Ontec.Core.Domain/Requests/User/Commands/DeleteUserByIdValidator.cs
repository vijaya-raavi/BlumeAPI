using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class DeleteUserByIdValidator : AbstractValidator<DeleteUserById>
    {
        public DeleteUserByIdValidator(IUserRepository _userRepository, IWorkContext _workContext) 
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(DeleteUserById.Id), string.Format(CommonConstants.NotExist, nameof(DeleteUserById.Id)));
                else
                {
                    if (model.Id != _workContext.CurrentUserId)
                    {
                        context.AddFailure(nameof(DeleteUserById.Id), "You are not authorize.");
                    }
                }

            });
        }
    }
}
