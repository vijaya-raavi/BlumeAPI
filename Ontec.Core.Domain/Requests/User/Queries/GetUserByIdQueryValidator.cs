using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Queries
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.Id).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetUserByIdQuery.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isxist = await _userRepository.IsUserIdExist(model.Id).ConfigureAwait(false);
                if (!isxist)
                {
                    context.AddFailure(nameof(GetUserByIdQuery.Id), string.Format(CommonConstants.NotExist, nameof(GetUserByIdQuery.Id)));
                }
            });
        }
    }
}
