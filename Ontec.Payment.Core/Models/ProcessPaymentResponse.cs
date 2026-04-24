using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class ProcessPaymentResponse
    {
        public string Status { get; set; }
        public string TransactionReferenceNumber { get; set; }
    }
}
