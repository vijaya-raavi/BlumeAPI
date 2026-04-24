using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Operator.Command;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class DeleteNotificationByIdQueryValidator : AbstractValidator<DeleteNotificationByIdQuery>
    {
        public DeleteNotificationByIdQueryValidator(INotificationRepository _notificationRepository)
        {
            RuleFor(m => m.Ids).NotNull();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (model.Ids.Any())
                {
                    foreach (var id in model.Ids)
                    {
                        var isExist = await _notificationRepository.IsNotificationsExist(id).ConfigureAwait(false);
                        if (isExist == 0)
                            context.AddFailure(nameof(DeleteNotificationByIdQuery.Ids), string.Format(CommonConstants.NotExist, (nameof(DeleteNotificationByIdQuery.Ids)+id)));
                    } 
                }
            });
        }
    }
}
