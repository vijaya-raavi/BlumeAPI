using Ontec.Payment.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Interfaces
{
    public interface IRechargeService
    {
        Task<RechargeResponse> RechargeAsync(RechargeRequest request);
        Task SaveTransactionAsync(string transactionReferenceNumber, RechargeResponse response);
    }
}
