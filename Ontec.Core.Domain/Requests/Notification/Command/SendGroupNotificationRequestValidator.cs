using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Notifiation;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class SendGroupNotificationRequestValidator : AbstractValidator<SendGroupNotificationRequest>
    {
        public SendGroupNotificationRequestValidator(IWorkContext workContext,INotificationRepository notificationRepository)
        {

            RuleFor(x => x.GroupId).NotNullAndEmptyAsync();
            RuleFor(m => m.Title).NotNullAndEmptyAsyncForProperty();
            RuleFor(m => m.Body).NotNullAndEmptyAsyncForProperty();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentRoleId).SplitPascalCase()));
                }
                int count = await notificationRepository.IsGroupIdExist(model.GroupId).ConfigureAwait(false);
                if (count == 0)
                {
                    context.AddFailure(nameof(SendGroupNotifications.GroupId), string.Format(CommonConstants.NotExist, nameof(SendGroupNotifications.GroupId).SplitPascalCase()));
                }
            });

        }
    }
}
