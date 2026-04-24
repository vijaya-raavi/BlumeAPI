using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMeterExpiringListRequestValidator : AbstractValidator<GetMeterExpiringListRequest>
    {
        public GetMeterExpiringListRequestValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.UserId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetMeterExpiringListRequest.UserId).SplitPascalCase(), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isxist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isxist)
                {
                    context.AddFailure(nameof(GetMeterExpiringListRequest.UserId), string.Format(CommonConstants.NotExist, nameof(GetMeterExpiringListRequest.UserId)));
                }
            });
        }
    }
}
