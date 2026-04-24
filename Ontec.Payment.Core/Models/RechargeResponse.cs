using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class RechargeResponse : BaseResponse
    {
        public string TransactionNumber { get; set; }
        public DateTime TransactionDate { get; set; }
    }


}
