using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class AddOrUpdateNotificationsQueryValidator : AbstractValidator<AddOrUpdateNotificationsQuery>
    {
        public AddOrUpdateNotificationsQueryValidator(IWorkContext _workContext, INotificationRepository _notificationRepository, IUserRepository _userRepository)
        {
            RuleFor(x => x.UserID).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(AddOrUpdateNotificationsQuery.UserID).SplitPascalCase(), 1);
            RuleFor (x=>x.Title).NotNull().IsValidInput();
            RuleFor(x => x.Description).NotNull().IsValidInput();
            RuleFor(x => x.NotificationType).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(AddOrUpdateNotificationsQuery.NotificationType).SplitPascalCase(), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist= await _userRepository.IsUserIdExist(model.UserID).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(AddOrUpdateNotificationsQuery.UserID), string.Format(CommonConstants.NotExist, nameof(AddOrUpdateNotificationsQuery.UserID)));
                }
                var isNotificationTypeExist = await _notificationRepository.IsNotificationTypeExist(model.NotificationType).ConfigureAwait(false);
                if (!isNotificationTypeExist)
                {
                    context.AddFailure(nameof(AddOrUpdateNotificationsQuery.NotificationType), string.Format(CommonConstants.NotExist, nameof(AddOrUpdateNotificationsQuery.NotificationType)));
                }
                if (model.UserID == _workContext.CurrentUserId || _workContext.CurrentRoleId!= (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(AddOrUpdateNotificationsQuery.UserID), string.Format(CommonConstants.Unauthorized, nameof(AddOrUpdateNotificationsQuery.UserID)));
                }
            });
        }
    }
}