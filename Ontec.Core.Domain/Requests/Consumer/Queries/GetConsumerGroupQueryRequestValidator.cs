using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Requests.Dashboard.Queries;
using Ontec.Core.Domain.Requests.Estate.Command;

namespace Ontec.Core.Domain.Requests.Consumer.Queries
{
    public class GetConsumerGroupQueryRequestValidator : AbstractValidator<GetConsumerGroupQueryRequest>
    {
        public GetConsumerGroupQueryRequestValidator(IConsumerRepository consumerRepository,IWorkContext workContext)
        {
            RuleFor(m => m.UserId).NotNull();

            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                if (workContext != null && workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(GetConsumerGroupQueryRequest.UserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId).SplitPascalCase()));
                }
                if (model.UserId > 0)
                {
                    var isExist = await consumerRepository.IsConsumerExist(model.UserId).ConfigureAwait(false);
                    if (!isExist)
                        context.AddFailure(nameof(GetConsumerGroupQueryRequest.UserId), "User id does not exist");
                }
            });
        }
    }
}
