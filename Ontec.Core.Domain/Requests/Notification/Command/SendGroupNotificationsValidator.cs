using System.Threading;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Models.Dto.Consumption;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class SendGroupNotificationsValidator : AbstractValidator<SendGroupNotifications>
    {
        public SendGroupNotificationsValidator(INotificationRepository notificationRepository, IWorkContext workContext)
        {
            RuleFor(x => x.GroupId).NotNullAndEmptyAsync();
            RuleFor(x => x.Title).NotNullAndEmptyAsync();
            RuleFor(x => x.Body).NotNullAndEmptyAsync();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
                {
                    if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                    {
                        context.AddFailure(nameof(workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentRoleId)));
                    }
                    int count = await notificationRepository.IsGroupIdExist(model.GroupId).ConfigureAwait(false);
                    if (count == 0)
                    {
                        context.AddFailure(nameof(SendGroupNotifications.GroupId), string.Format(CommonConstants.NotExist, nameof(SendGroupNotifications.GroupId)));
                    }
                });

        }
    }
}
