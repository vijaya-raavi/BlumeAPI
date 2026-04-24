using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class RechargeRequest
    {
        public string MeterNumber { get; set; }
        public double Amount { get; set; }
    }
}
