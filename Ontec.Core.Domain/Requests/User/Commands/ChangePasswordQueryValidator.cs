using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class ChangePasswordQueryValidator : AbstractValidator<ChangePasswordQuery>
    {
        public ChangePasswordQueryValidator(IUserRepository _userRepository, IWorkContext _workContext)
        {
            RuleFor(x => x.UserId).GreaterThanOrEqualToAsync(nameof(ChangePasswordQuery.UserId), 1);
            RuleFor(x => x.OldPassword).IsValidPassword().NotNullAndEmptyAsync().GreaterThanOrEqualToAsyncOldPassword(nameof(ChangePasswordQuery.OldPassword).SplitPascalCase(),8).LengthShouldBeLessOrEqualToAsync(nameof(ChangePasswordQuery.OldPassword).SplitPascalCase(), 15);
            RuleFor(x => x.NewPassword).NotNullAndEmptyAsync().IsValidPassword().GreaterThanOrEqualToAsync(nameof(ChangePasswordQuery.NewPassword).SplitPascalCase(), 8) .LengthShouldBeLessOrEqualToAsync(nameof(ChangePasswordQuery.NewPassword).SplitPascalCase(), 15);
            RuleFor(x => x.ConfirmPassword).NotNullAndEmptyAsync().IsValidPassword().GreaterThanOrEqualToAsync(nameof(ChangePasswordQuery.ConfirmPassword).SplitPascalCase(), 8).LengthShouldBeLessOrEqualToAsync(nameof(ChangePasswordQuery.ConfirmPassword).SplitPascalCase(), 15);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);

                if (!isExist)
                {
                    context.AddFailure(nameof(ChangePasswordQuery.UserId), string.Format(CommonConstants.NotExist, nameof(ChangePasswordQuery.UserId)));
                }
                
                if (model.NewPassword == model.OldPassword)
                {
                    context.AddFailure(nameof(ChangePasswordQuery.NewPassword), string.Format(CommonConstants.NotAllowed, nameof(ChangePasswordQuery.NewPassword).SplitPascalCase()));
                }
                else
                {
                    if (_workContext.CurrentUserId != model.UserId)
                    {
                        context.AddFailure(nameof(ChangePasswordQuery.UserId), string.Format(CommonConstants.Unauthorized, nameof(ChangePasswordQuery.UserId).SplitPascalCase()));
                    }
                    if (model.NewPassword != model.ConfirmPassword)
                    {
                        context.AddFailure(nameof(ChangePasswordQuery.ConfirmPassword), string.Format(CommonConstants.NotMatch, nameof(ChangePasswordQuery.ConfirmPassword).SplitPascalCase()));
                    }
                }
            });
        }
    }
}
