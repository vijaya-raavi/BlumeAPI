using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Models
{
    public class BaseResponse
    {
        public string Status { get; set; }
        public ErrorInfo ErrorInfo { get; set; }
        public List<string> Info { get; set; }
    }

    public class ErrorInfo {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
