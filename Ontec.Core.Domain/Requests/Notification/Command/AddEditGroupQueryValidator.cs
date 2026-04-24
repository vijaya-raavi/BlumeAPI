using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Notifiation;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public  class AddEditGroupQueryValidator:AbstractValidator<AddEditGroupQuery>
    {
        public AddEditGroupQueryValidator(INotificationRepository notificationRepository,IWorkContext workContext)
        {
            RuleFor(x => x.GroupName).NotNullAndEmptyAsync().IsValidGroupName(nameof(AddEditGroupQuery.GroupName).SplitPascalCase()).GreaterThanOrEqualToAsync(nameof(AddEditGroupQuery.GroupName).SplitPascalCase(), 2).LengthShouldBeLessOrEqualToAsync(nameof(AddEditGroupQuery.GroupName).SplitPascalCase(), 100);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentRoleId)));
                }
                if (model.Id >0)
                {
                    var isExist = await notificationRepository.IsGroupIdExist(model.Id).ConfigureAwait(false);
                    if (isExist == 0)
                    {
                        context.AddFailure(nameof(AddEditGroupQuery.Id), string.Format(CommonConstants.NotExist, nameof(AddEditGroupQuery.Id)));
                    }

                }
                if (model.Id == 0)
                {
                    var isExist = await notificationRepository.IsGroupExist(model.GroupName.ToLower()).ConfigureAwait(false);
                    if (isExist > 0)
                    {
                        context.AddFailure(nameof(AddEditGroupQuery.GroupName), string.Format(CommonConstants.AlreadyExist, nameof(AddEditGroupQuery.GroupName).SplitPascalCase()));
                    }

                }

            });
        }
    }
}
