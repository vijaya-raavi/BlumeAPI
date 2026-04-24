using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public class UpdateConsumerQueryValidator : AbstractValidator<UpdateConsumerQuery>
    {
        public UpdateConsumerQueryValidator(IConsumerRepository _consumerRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualToAsync(nameof(UpdateConsumerQuery.Id), 1);
            RuleFor(m => m.FirstName).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(UpdateConsumerQuery.FirstName).SplitPascalCase(), 10);
            RuleFor(m => m.LastName).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(UpdateConsumerQuery.LastName).SplitPascalCase(), 10);
            RuleFor(x => x.MobileNumber).NotNullAndEmptyAsync().IsValidMobile().LengthShouldBeEqualAsync(nameof(UpdateConsumerQuery.MobileNumber).SplitPascalCase(), 10);
            RuleFor(x => x.Address).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(UpdateConsumerQuery.Address), 50);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
                {
                    var isExist = await _consumerRepository.IsConsumerExist(model.Id).ConfigureAwait(false);
                    if (!isExist)
                    {
                        context.AddFailure(nameof(UpdateConsumerQuery.Id), string.Format(CommonConstants.NotExist, nameof(UpdateConsumerQuery.Id)));
                    }
                });
        }
    }
}
