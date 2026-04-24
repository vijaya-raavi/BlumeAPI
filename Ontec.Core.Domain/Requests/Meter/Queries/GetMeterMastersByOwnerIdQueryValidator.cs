using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMeterMastersByOwnerIdQueryValidator : AbstractValidator<GetMeterMastersByOwnerIdQuery>
    {
        public GetMeterMastersByOwnerIdQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.OwnerId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetMeterMastersByOwnerIdQuery.OwnerId), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.OwnerId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetMeterMastersByOwnerIdQuery.OwnerId), string.Format(CommonConstants.NotExist, nameof(GetMeterMastersByOwnerIdQuery.OwnerId)));
            });
        }
    }
}
