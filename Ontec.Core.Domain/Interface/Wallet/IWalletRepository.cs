using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.Wallet.Command;
using Ontec.Core.Domain.Requests.Wallet.Queries;
using WalletModel = Ontec.Core.Domain.Models.Dto.Wallet.Wallet;

namespace Ontec.Core.Domain.Interface.Wallet
{
    public  interface IWalletRepository
    {
        #region UpdateWalletBalance
        Task<int> IsUserIdExistInWallet(int userId);
        Task<int> AddUserWallet(int userId);
        Task<int> UpdateUserWallet(UpdateWalletBalanceQuery request,  double WalletBalance);
        Task<UserWalletDto> GetUserWalletById(int userId);
        Task<int> AddUserWalletTransaction(AddUserWalletTransactionQuery walletTransaction);
        Task<IEnumerable<UserWalletTransactionDto>> GetUserWalletTransaction(GetUserWalletTransactionQuery request);

        #endregion
        Task CreateWalletAsync(int userId);
        Task<double> GetBalanceAsync(int userId);
        Task<WalletModel> GetWalletAsync(int userId);
        Task<double> AddBalanceAsync(int userId, double amountToAdd, double walletBalance, string transactionRemark, bool useWallet, double topupAmount,string txnId);
        Task<double> DeductBalanceAsync(int userId, double amountToDeduct, double walletBalance, string transactionRemark, bool useWallet,string txnId);
        Task<double> RefundBalanceAsync(int userId, double totalAmount,string txnId);
        bool WalletExists(int userId);
        Task saveTransaction(int transactionType, double transactionAmount, string remark, int walletId,string txnId);
        Task<int> IsWalletExist(int userId);

    }
}
