using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public  class GetUserSavedCardQueryValidator : AbstractValidator<GetUserSavedCardsQuery>
    {
        public GetUserSavedCardQueryValidator(IUserRepository _userRepository)
                                                    
        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(GetUserSavedCardsQuery.UserId), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetUserSavedCardsQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetUserSavedCardsQuery.UserId)));
            });
        }
    }
}
