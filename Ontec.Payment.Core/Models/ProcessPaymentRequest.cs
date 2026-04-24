using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class ProcessPaymentRequest
    {
        public int UserId {  get; set; }
        public string MeterNumber { get; set; }
        public string TransactionId { get; set; }
        public double TransactionAmount { get; set; }        
        public string TransactionCurrency { get; set; }
        public Breakup PaymentBreakup { get; set; }
        public string PreviousChecksum { get; set; }
    }    
}
