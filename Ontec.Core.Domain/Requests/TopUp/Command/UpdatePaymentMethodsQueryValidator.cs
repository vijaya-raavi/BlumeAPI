using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public  class UpdatePaymentMethodsQueryValidator : AbstractValidator<UpdatePaymentMethodsQuery>
    {
        public UpdatePaymentMethodsQueryValidator(IUserRepository userRepository, IWorkContext workContext)
        {
            
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await userRepository.IsUserIdExist(workContext.CurrentUserId).ConfigureAwait(false);
                
                if (!isExist)
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.NotExist, nameof(workContext.CurrentUserId)));
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId)));
                }
                foreach (var item in model.PaymentMethods)
                {
                    if(item != null)
                    {
                        if (item.Id != 0)
                        {
                            if (item.Percentage > 100)
                            {
                                context.AddFailure(nameof(item.Percentage), "Percentage should be less than 100");
                            }
                            if (item.Discount > 100)
                            {
                                context.AddFailure(nameof(item.Discount), "Discount should be less than 100");
                            }
                        }
                        else
                        {
                            context.AddFailure(nameof(item.Id), string.Format(CommonConstants.NotExist, nameof(item.Id)));
                        }
                    }
                    
                }

            });
        }
    }
}
