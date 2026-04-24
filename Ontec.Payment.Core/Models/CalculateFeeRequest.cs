using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class CalculateFeeRequest
    {
        public int UserId { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
}
