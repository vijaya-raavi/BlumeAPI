using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Notification.Queries
{
    public  class GetNotificationsByUserIdQueryValidator :AbstractValidator<GetNotificationsByUserIdQuery>
    {
        public GetNotificationsByUserIdQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isExist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(GetNotificationsByUserIdQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetNotificationsByUserIdQuery.UserId)));
                }
            });
        }
    }
}
