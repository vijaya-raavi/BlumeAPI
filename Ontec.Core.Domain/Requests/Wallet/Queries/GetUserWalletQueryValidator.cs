using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Wallet.Queries
{
    public class GetUserWalletQueryValidator : AbstractValidator<GetUserWalletQuery>
    {
        public GetUserWalletQueryValidator(IUserRepository _userRepository)

        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(GetUserWalletQuery.UserId), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetUserWalletQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetUserWalletQuery.UserId)));
            });
        }
    }
}
