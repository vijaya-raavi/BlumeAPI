using Ontec.Payment.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core
{
    public class FeeHelper
    {
        public static Breakup GetRechargeBreakup(double walletBalance, double transactionValue, int processingFeeInPercent)
        {
            var rechargeAmountBreakup = new Breakup();

            rechargeAmountBreakup.TotalAmount = walletBalance + transactionValue;
            rechargeAmountBreakup.Amount = Math.Round((double)(rechargeAmountBreakup.TotalAmount/((double)(100+processingFeeInPercent)/100)), 2);
            rechargeAmountBreakup.Fee = rechargeAmountBreakup.TotalAmount - rechargeAmountBreakup.Amount;

            return rechargeAmountBreakup;
        }
    }
}
