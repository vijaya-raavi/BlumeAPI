using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetPropertyDetailsQueryValidator : AbstractValidator<GetPropertyDetailsQuery>
    {
        public GetPropertyDetailsQueryValidator(IUserRepository _userRepository)

        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(GetPropertyDetailsQuery.UserId), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertyDetailsQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetPropertyDetailsQuery.UserId)));
            });
        }
    }
}
