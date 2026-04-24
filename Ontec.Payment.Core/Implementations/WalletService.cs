using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Payment.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core.Implementations
{
    public class WalletService : IWalletService
    {
        private readonly int _userId;
        private readonly IWalletRepository _walletRepository;
        private readonly int _transactionType;


        public WalletService(int userId, IWalletRepository walletRepository)
        {
            _userId=userId;
            _walletRepository=walletRepository;
            checkAndCreateWallet();
        }

        private void checkAndCreateWallet()
        {
            if (!_walletRepository.WalletExists(_userId))
                _walletRepository.CreateWalletAsync(_userId);
        }

        public async Task<double> AddBalanceAsync(double amountToAdd,double walletBalance)
        {
            return await _walletRepository.AddBalanceAsync( _userId, amountToAdd, walletBalance,"");
        }

        public async Task<double> DeductBalanceAsync(double amountToDeduct,double walletBalance)
        {
            return await _walletRepository.DeductBalanceAsync( _userId, amountToDeduct, walletBalance, "");
        }

        public async Task<double> GetBalanceAsync()
        {
            return await _walletRepository.GetBalanceAsync(_userId);
        }

        public async Task<double> RefundAsync(double totalAmount)
        {
            return await _walletRepository.RefundBalanceAsync(_userId, totalAmount);
        }
    }
}
