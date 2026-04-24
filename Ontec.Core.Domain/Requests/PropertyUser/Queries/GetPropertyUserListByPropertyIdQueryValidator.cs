using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Requests.Property.Handler;

namespace Ontec.Core.Domain.Requests.PropertyUser.Queries
{
    public class GetPropertyUserListByPropertyIdQueryValidator : AbstractValidator<GetPropertyUserListByPropertyIdQuery>
    {
        public GetPropertyUserListByPropertyIdQueryValidator(IPropertyRepository _propertyRepository)
        {
            RuleFor(m => m.PropertyId).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertyByIdQuery.Id), string.Format(CommonConstants.NotExist, nameof(GetPropertyByIdQuery.Id)));
            });
        }
    }
}
