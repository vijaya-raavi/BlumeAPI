using Ontec.Core.Domain.BankNotification;

namespace Ontec.Notification.Core.Interfaces
{
    public interface INotificationProcessor
    {
        Task ProcessAsync(BankNotificationDetails notification);
    }
}
