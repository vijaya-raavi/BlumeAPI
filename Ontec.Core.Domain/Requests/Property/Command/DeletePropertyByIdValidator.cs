using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Property.Command
{
    public class DeletePropertyByIdValidator : AbstractValidator<DeletePropertyById>
    {
        public DeletePropertyByIdValidator(IPropertyRepository _propertyRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
                {
                    var isValid = await _propertyRepository.IsPropertyExist(model.Id).ConfigureAwait(false);
                    if (!isValid)
                        context.AddFailure(nameof(DeletePropertyById.Id), string.Format(CommonConstants.NotExist, nameof(DeletePropertyById.Id)));
                });
        }
    }
}


