using Ontec.Payment.Core.Interfaces;
using Ontec.Payment.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Implementations
{
    public class RechargeService : IRechargeService
    {
        public async Task<RechargeResponse> RechargeAsync(RechargeRequest request)
        {
            //TODO: actual implementation needed. currently dummy responses added
            return new RechargeResponse() { Status = "Success" };

        }

        public async Task SaveTransactionAsync(string transactionReferenceNumber, RechargeResponse response)
        {
            //TODO: actual implementation needed. currently dummy responses added
        }
    }
}
