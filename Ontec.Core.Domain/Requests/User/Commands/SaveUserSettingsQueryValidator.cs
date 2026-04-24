using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class SaveUserSettingsQueryValidator: AbstractValidator<SaveUserSettingsQuery>
    {
        public SaveUserSettingsQueryValidator(IUserRepository _userRepository,IWorkContext _workContext) 
        {
            RuleFor(x => x.UserId).GreaterThanOrEqualToAsync(nameof(SaveUserSettingsQuery.UserId), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(SaveUserSettingsQuery.UserId), string.Format(CommonConstants.NotExist, nameof(SaveUserSettingsQuery.UserId).SplitPascalCase()));
                }
                else
                {
                    if (_workContext.CurrentUserId!= model.UserId)
                    {
                        context.AddFailure(nameof(_workContext.CurrentUserId), "You are not authorize.");
                    }
                }
            });

        }
    }
}
