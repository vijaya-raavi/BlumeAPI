using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.PropertyUser.Queries
{
    public class GetPropertyUserMastersByOwnerIdQueryValidator : AbstractValidator<GetPropertyUserMastersByOwnerIdQuery>
    {
        public GetPropertyUserMastersByOwnerIdQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.OwnerId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetPropertyUserMastersByOwnerIdQuery.OwnerId), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.OwnerId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertyUserMastersByOwnerIdQuery.OwnerId), string.Format(CommonConstants.NotExist, nameof(GetPropertyUserMastersByOwnerIdQuery.OwnerId)));
            });
        }
    }
}
