using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.PropertyUser;

namespace Ontec.Core.Domain.Requests.PropertyUser.Queries
{
    public class GetPropertyUserByIdValidator : AbstractValidator<GetPropertyUserById>
    {
        public GetPropertyUserByIdValidator(IPropertyUserRepository _propertyUserRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyUserRepository.IsPropertyUserIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertyUserById.Id), string.Format(CommonConstants.NotExist, nameof(GetPropertyUserById.Id)));
            });
        }
    }
}