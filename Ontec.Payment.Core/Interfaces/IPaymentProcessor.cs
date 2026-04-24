using Ontec.Payment.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Interfaces
{
    public interface IPaymentProcessor
    {
        Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request);
        Task<PaymentWithFeeResponse> CalculateFee(CalculateFeeRequest request);
    }
}
