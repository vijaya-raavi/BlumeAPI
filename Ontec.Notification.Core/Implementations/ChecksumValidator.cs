using Ontec.Core.Domain.BankNotification;
using Ontec.Notification.Core.Interfaces;
using Ontec.Notification.Core.Models;
using Ontec.Payment.Core;
using System.Text;

namespace Ontec.Notification.Core.Implementations
{
    public class ChecksumValidator : INotificationValidator
    {
            public NotificationValidationResult Validate(BankNotificationDetails notification)
            {
                var validationResult = new NotificationValidationResult();
                if (populateChecksum(notification) == notification.NetUpChecksum) {
                    validationResult.IsValid = true;
                }

                return validationResult;
            }

            private string populateChecksum(BankNotificationDetails notification)
            {
                StringBuilder checksumStringBuilder = new StringBuilder();
                checksumStringBuilder.Append(notification.TransactionValue.ToString("N2"));
                checksumStringBuilder.Append(notification.IsCreditTransaction ? "c" : "d");
                checksumStringBuilder.Append(notification.AccountNumber);
                checksumStringBuilder.Append(notification.PayerReferenceNumber);

                var checkSumString = checksumStringBuilder.ToString();
                return ChecksumHelper.PopulateChecksum(checkSumString);
            }

        
    }
}
