using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Payment.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Implementations
{
    public class WalletServiceFactory : IWalletServiceFactory
    {
        private readonly IWalletRepository _walletRepository;

        public WalletServiceFactory(IWalletRepository walletRepository)
        {
            _walletRepository=walletRepository;
        }

        public async Task<IWalletService> GetWalletServiceInstanceAsync(int userId)
        {
            return new WalletService(userId, _walletRepository);
        }
    }
}
