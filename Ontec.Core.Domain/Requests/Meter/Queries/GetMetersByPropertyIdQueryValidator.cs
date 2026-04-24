using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMetersByPropertyIdQueryValidator : AbstractValidator<GetMetersByPropertyIdQuery>
    {
        public GetMetersByPropertyIdQueryValidator(IPropertyRepository _propertyRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyRepository.IsPropertyIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetMetersByPropertyIdQuery.Id), string.Format(CommonConstants.NotExist, nameof(GetMetersByPropertyIdQuery.Id)));
            });
        }
    }
}
