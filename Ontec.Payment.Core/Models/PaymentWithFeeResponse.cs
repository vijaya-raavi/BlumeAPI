using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class PaymentWithFeeResponse
    {
        public double WalletBalance { get; set; }
        public double BaseAmount { get; set; }
        public double Fee { get; set; }
        public string Currency { get; set; }
        public double TotalAmount { get { return BaseAmount + Fee; } }
        public string Checksum { get; set; }
    }
}
