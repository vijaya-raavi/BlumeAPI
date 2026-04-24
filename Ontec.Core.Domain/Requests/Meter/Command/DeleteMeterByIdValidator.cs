using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class DeleteMeterByIdValidator : AbstractValidator<DeleteMeterById>
    {
        public DeleteMeterByIdValidator(IMeterRepository _meterRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _meterRepository.IsMeterIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(DeleteMeterById.Id), string.Format(CommonConstants.NotExist, nameof(DeleteMeterById.Id)));
            });
        }
    }
}
