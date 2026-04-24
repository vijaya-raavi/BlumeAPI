using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Core.Domain.BankNotification
{
    public class BankNotificationDetails
    {
        public string BankCode { get; set; }
        public string NetUpTransactionGuid { get; set; }
        public string SourceDeliveryService { get; set; }
        public DateTime InitialRecieveDateTime { get; set; }
        public object ForwardedDateTime { get; set; }
        public string BankTransactionId { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public bool IsCreditTransaction { get; set; }
        public double TransactionValue { get; set; }
        public double TransactionFeesValue { get; set; }
        public string PayerReferenceNumber { get; set; }
        public double AccountBalance { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public DateTime EffectiveDateTime { get; set; }
        public bool PendingClearance { get; set; }
        public string TransactionDescription { get; set; }
        public string PaymentChannel { get; set; }
        public string BankProvidedChecksum { get; set; }
        public bool BankChecksumMatched { get; set; }
        public string NetUpChecksum { get; set; }
    }
}
