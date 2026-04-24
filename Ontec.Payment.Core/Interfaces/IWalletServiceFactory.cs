using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Interfaces
{
    public interface IWalletServiceFactory
    {
        Task<IWalletService> GetWalletServiceInstanceAsync(int userId);
    }
}
