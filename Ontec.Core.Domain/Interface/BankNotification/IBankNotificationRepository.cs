using Ontec.Core.Domain.Interface;

namespace Ontec.Core.Domain.BankNotification
{
    public interface IBankNotificationRepository
    {
        Task SaveErrorAsync(BankNotificationDetails notification);
    }
}
