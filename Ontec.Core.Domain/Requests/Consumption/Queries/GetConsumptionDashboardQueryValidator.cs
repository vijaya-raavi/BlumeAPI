using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Meter;

namespace Ontec.Core.Domain.Requests.Consumption.Queries
{
    public class GetConsumptionDashboardQueryValidator : AbstractValidator<GetConsumptionDashboardQuery>
    {
        public GetConsumptionDashboardQueryValidator(IMeterRepository _meterRepository)
        {
            RuleFor(m => m.MeterId).NotNullAndEmptyAsync();
            RuleFor(m => m.ConsumptionCylceTypeId).GreaterThanOrEqualToAsync(nameof(GetConsumptionDashboardQuery.MeterId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isMeterIdExist = await _meterRepository.GetTargetConsumptionByMeterNumber(model.MeterId).ConfigureAwait(false);
                if (isMeterIdExist == null)
                {
                    context.AddFailure(model.MeterId, string.Format(CommonConstants.NotExist, nameof(GetConsumptionDashboardQuery.MeterId)));
                }
                if (isMeterIdExist != null && string.IsNullOrEmpty(isMeterIdExist.DailyTargetConsumption))
                {
                    context.AddFailure(model.MeterId, string.Format(CommonConstants.NotExist, nameof(GetConsumptionDashboardQuery.MeterId)));
                }
            });
        }
    }
}