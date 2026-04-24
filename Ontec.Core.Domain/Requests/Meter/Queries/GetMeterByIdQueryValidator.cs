using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMeterByIdQueryValidator : AbstractValidator<GetMeterByIdQuery>
    {
        public GetMeterByIdQueryValidator(IMeterRepository _meterRepository)
        {
            RuleFor(x => x.Id).NotNull().GreaterThanOrEqualToAsync(nameof(GetMeterByIdQuery.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _meterRepository.IsMeterIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                {
                    context.AddFailure(nameof(GetMeterByIdQuery.Id),string.Format(CommonConstants.NotExist, nameof(GetMeterByIdQuery.Id)));
                }
            });
        }
    }
}
