using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.ManagePermissions.Command
{
    public class ManagePermissionQueryValidator : AbstractValidator<ManagePermissionQuery>
    {
        public ManagePermissionQueryValidator(IUserRepository userRepository, IWorkContext workContext)
        {
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await userRepository.IsUserIdExist(workContext.CurrentUserId).ConfigureAwait(false);

                if (!isExist)
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.NotExist, nameof(workContext.CurrentUserId)));
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId)));
                }
                foreach (var item in model.Permissions)
                {
                    if (item != null)
                    {
                        if (item.Id == 0)
                        {                          
                            context.AddFailure(nameof(item.Id), string.Format(CommonConstants.NotExist, nameof(item.Id)));
                        }
                    }

                }

            });
        }
    }
}
