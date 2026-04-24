using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Requests.Consumer.Queries;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public class SendNotifcationToConsumerRequestValidator:AbstractValidator<SendNotifcationToConsumerRequest>
    {
        public SendNotifcationToConsumerRequestValidator(IConsumerRepository consumerRepository,IWorkContext workContext)
        {
            RuleFor(m => m.ConsumerId).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m.Title).NotNullAndEmptyAsyncForProperty();
            RuleFor(m => m.Body).NotNullAndEmptyAsyncForProperty();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (workContext != null && workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(GetConsumerGroupQueryRequest.UserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId).SplitPascalCase()));
                }
                var isValid = await consumerRepository.IsConsumerExist(model.ConsumerId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(SendNotifcationToConsumerRequest.ConsumerId), "Conusmer id does not exist.");
            });
        }
    }
}
