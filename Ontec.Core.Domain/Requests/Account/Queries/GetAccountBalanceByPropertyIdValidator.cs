using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Requests.Dashboard.Queries;

namespace Ontec.Core.Domain.Requests.Account.Queries
{
    public  class GetAccountBalanceByPropertyIdValidator : AbstractValidator<GetAccountBalanceByPropertyId>
    {
        public GetAccountBalanceByPropertyIdValidator(IPropertyRepository _propertyRepository)
        {
            RuleFor(x => x.PropertyId).NotNull().GreaterThanOrEqualToAsync(nameof(GetAccountBalanceByPropertyIdQuery.PropertyId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetAccountBalanceByPropertyId.PropertyId), "Property id does not exist");

            });
        }
    }
}
