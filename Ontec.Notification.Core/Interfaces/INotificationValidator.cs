using Ontec.Core.Domain.BankNotification;
using Ontec.Notification.Core.Models;

namespace Ontec.Notification.Core.Interfaces
{
    public interface INotificationValidator
    {
        NotificationValidationResult Validate(BankNotificationDetails notification);
    }
}
