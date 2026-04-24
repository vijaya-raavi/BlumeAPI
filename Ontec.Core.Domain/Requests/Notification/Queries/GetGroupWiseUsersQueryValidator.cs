using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Notifiation;

namespace Ontec.Core.Domain.Requests.Notification.Queries
{
    public class GetGroupWiseUsersQueryValidator : AbstractValidator<GetGroupWiseUsersQuery>
    {
        public GetGroupWiseUsersQueryValidator(INotificationRepository notificationRepository)
        {
            //RuleFor(x => x.GroupId).NotNull();
            //RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            //{
            //    int id = await notificationRepository.IsGroupIdExist(model.GroupId).ConfigureAwait(false);
            //    if (id == 0)
            //    {

            //        context.AddFailure(nameof(GetGroupWiseUsersQuery.GroupId), string.Format(CommonConstants.NotExist, nameof(GetGroupWiseUsersQuery.GroupId)));

            //    }
            //});
        }
    }
}
