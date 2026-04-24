using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Interfaces
{
    public interface IWalletService
    {
        Task<double> GetBalanceAsync();
        Task<double> AddBalanceAsync(double amountToAdd,double walletBalance);
        Task<double> DeductBalanceAsync(double amountToDeduct,double walletBalance);
        Task<double> RefundAsync(double totalAmount);
    }
}
