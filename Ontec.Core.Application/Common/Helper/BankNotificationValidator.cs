using Ontec.Core.Domain.BankNotification;
using Ontec.Core.Domain.Requests.Debitech.Command;
using System.Globalization;
using System.Text;

namespace Ontec.Core.Application.Common.Helper
{
    public interface IBankNotificationValidator
    {
        bool Validate(BankNotificationRequest notification);
    }
    public class BankNotificationValidator : IBankNotificationValidator
    {
        public bool Validate(BankNotificationRequest notification)
        {
           bool isValid = false;
            var checkSum = populateChecksum(notification);
            if (checkSum== notification.NetUpChecksum)
            {
                isValid = true;
            }

            return isValid;
        }

        private string populateChecksum(BankNotificationRequest notification)
        {
            StringBuilder checksumStringBuilder = new StringBuilder();
            checksumStringBuilder.Append(notification.TransactionValue.ToString("F2", CultureInfo.InvariantCulture));
            checksumStringBuilder.Append(notification.IsCreditTransaction ? "c" : "d");
            checksumStringBuilder.Append(notification.AccountNumber);
            checksumStringBuilder.Append(notification.PayerReferenceNumber);

            //var checkSumString = checksumStringBuilder.ToString();
            var checkSumString = checksumStringBuilder.ToString().Normalize(NormalizationForm.FormC);
            return ChecksumHelper.PopulateChecksum(checkSumString);
        }

    }
}
