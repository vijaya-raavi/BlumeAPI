using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Estate;

namespace Ontec.Core.Domain.Requests.Estate.Command
{
    public  class SendEstateNotificationRequestValidator:AbstractValidator<SendEstateNotificationRequest>
    {
        public SendEstateNotificationRequestValidator(IWorkContext workContext, IEstateRepository estateRepository)
        {
            RuleFor(m => m.EstateId).NotNull();
            RuleFor(m => m.Topic).NotNullAndEmptyAsyncForProperty().LengthShouldBeLessOrEqualToAsync("Topic name should be less than or equal to ", 55);
            RuleFor(m => m.Title).NotNullAndEmptyAsyncForProperty().LengthShouldBeLessOrEqualToAsync("Title name should be less than or equal to ", 55);
            RuleFor(m => m.Body).NotNullAndEmptyAsyncForProperty().LengthShouldBeLessOrEqualToAsync("Body name should be less than or equal to ", 100);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentRoleId).SplitPascalCase()));
                }
                if (model.EstateId > 0)
                {
                    int count = await estateRepository.IsActiveEstateIdExist(model.EstateId).ConfigureAwait(false);
                    if (count == 0)
                        context.AddFailure(nameof(DeleteEstateRequestCommand.Id), "Estate id does not exist");
                }

            });

        }
    }
}
