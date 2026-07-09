using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Meter;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetCreditsByMeterIdQueryValidator : AbstractValidator<GetCreditsByMeterIdQuery>
    {
        public GetCreditsByMeterIdQueryValidator(IMeterRepository _meterRepository)
        {

            RuleFor(x => x.MeterId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetCreditsByMeterIdQuery.MeterId).SplitPascalCase(), 1);
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isMeterExist = await _meterRepository.IsMeterIdExist(model.MeterId).ConfigureAwait(false);
                if (!isMeterExist)
                    context.AddFailure(nameof(GetCreditsByMeterIdQuery.MeterId), string.Format(CommonConstants.NotExist, nameof(GetCreditsByMeterIdQuery.MeterId)));

                
            });
        }
    }
}
