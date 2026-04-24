using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class Breakup
    {
        /// <summary>
        /// This value indicates recharge amount + fee
        /// </summary>
        public double TotalAmount { get; set; }
        /// <summary>
        /// This value indicates recharge amount without fee
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// This value indicates fee applicable on recharge amount
        /// </summary>
        public double Fee { get; set; }

    }

    
}
