using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetDashboardMeterDayConsumptionQueryValidator : AbstractValidator<GetDashboardMeterDayConsumptionQuery>
    {
        public GetDashboardMeterDayConsumptionQueryValidator(IPropertyRepository _propertyRepository)
        {
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetDashboardMeterDayConsumptionQuery.PropertyId), string.Format(CommonConstants.NotExist, nameof(GetDashboardMeterDayConsumptionQuery.PropertyId)));
            });
        }
    }
}
