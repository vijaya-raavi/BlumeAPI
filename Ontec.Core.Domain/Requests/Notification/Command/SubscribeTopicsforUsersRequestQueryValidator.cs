using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Notification.Commands;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class SubscribeTopicsforUsersRequestQueryValidator : AbstractValidator<SubscribeTopicsforUsersRequestQuery>
    {
        public SubscribeTopicsforUsersRequestQueryValidator(INotificationRepository notificationRepository,
                                                            IUserRepository userRepository,IWorkContext workContext)
        {
            RuleFor(m => m.GroupId).NotNull();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentRoleId)));
                }
                int id=await notificationRepository.IsGroupIdExist(model.GroupId).ConfigureAwait(false);
                if (id == 0)
                {
                    context.AddFailure(nameof(AddCustomersInNotificationGroupsQuery.GroupId), string.Format(CommonConstants.NotExist, nameof(AddCustomersInNotificationGroupsQuery.GroupId)));
                }
                
                


            });
        }
    }
}
