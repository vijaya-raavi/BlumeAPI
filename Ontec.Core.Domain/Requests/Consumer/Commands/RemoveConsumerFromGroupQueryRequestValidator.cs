using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Requests.Consumer.Queries;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public class RemoveConsumerFromGroupQueryRequestValidator:AbstractValidator<RemoveConsumerFromGroupQueryRequest>
    {
        public RemoveConsumerFromGroupQueryRequestValidator(IConsumerRepository consumerRepository,IWorkContext workContext)
        {
            RuleFor(m => m.NotificationGroupLinkId).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (workContext != null && workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(GetConsumerGroupQueryRequest.UserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId).SplitPascalCase()));
                }
                int count = await consumerRepository.IsNotifcationGroupLinkIdExist(model.NotificationGroupLinkId).ConfigureAwait(false);
                if (count==0)
                    context.AddFailure(nameof(RemoveConsumerFromGroupQueryRequest.NotificationGroupLinkId), "Notifcation group link id does not exist.");
            });
        }
    }
}
