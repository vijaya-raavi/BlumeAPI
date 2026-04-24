using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class GetPaymentMethodsQueryValidator :AbstractValidator<GetPaymentMethodsQuery>
    {
        public GetPaymentMethodsQueryValidator(IUserRepository userRepository,IWorkContext workContext) 
        {
           
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await userRepository.IsUserIdExist(workContext.CurrentUserId).ConfigureAwait(false);
                if (!isExist)
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.NotExist, nameof(workContext.CurrentUserId)));
            });
        }
    }
}
