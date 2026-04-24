using FluentValidation;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Requests.Account.Queries;

namespace Ontec.Core.Domain.Requests.Account
{
    public class GetAccountHisotryQueryValidator : AbstractValidator<GetAccountHisotryQuery>
    {
        public GetAccountHisotryQueryValidator(IPropertyRepository _propertyRepository)
        {
            RuleFor(x => x.PropertyId).NotNull().GreaterThanOrEqualToAsync(nameof(GetAccountHisotryQuery.PropertyId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetAccountHisotryQuery.PropertyId), "Property id does not exist");

            });
        }
    }
}
