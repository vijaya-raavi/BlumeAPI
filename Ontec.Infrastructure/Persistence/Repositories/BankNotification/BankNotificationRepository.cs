using Ontec.Core.Domain.BankNotification;

namespace Ontec.Infrastructure.Persistence.Repositories.Notification
{
    public class BankNotificationRepository : IBankNotificationRepository
    {
        public Task SaveErrorAsync(BankNotificationDetails notification)
        {
            throw new NotImplementedException();
        }
    }
}
