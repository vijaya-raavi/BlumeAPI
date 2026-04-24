using Dapper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.Wallet.Command;
using Ontec.Core.Domain.Requests.Wallet.Queries;

namespace Ontec.Infrastructure.Persistence.Repositories.Wallet
{
    public class WalletRepository(IGenericRepository genericRepository) : IWalletRepository
    {
        private readonly IGenericRepository _genericRepository = genericRepository;

        #region UpdateWalletBalance
        public async Task<int> IsUserIdExistInWallet(int userId)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_user_wallet  
                          WHERE user_id=@UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<UserWalletDto> GetUserWalletById(int userId)
        {
            try
            {
                var GetWalletBalanceQuery = @"SELECT * from ohd_user_wallet where user_id=@UserId";
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                var result = await _genericRepository.GetFirstOrDefaultAsync<UserWalletDto>(GetWalletBalanceQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<int> IsWalletExist(int userId)
        {
            try
            {
                var GetWalletBalanceQuery = @"SELECT id from ohd_user_wallet where user_id=@UserId";
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                var result = await _genericRepository.GetFirstOrDefaultAsync<int>(GetWalletBalanceQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<int> AddUserWalletTransaction(AddUserWalletTransactionQuery walletTransaction)
        {
            var insertToWalletTransactionQuery = @" INSERT INTO public.ohd_wallet_transaction
                                       (wallet_id,                                                
                                         transaction_amount,
                                         transaction_type_id,
                                         updated_balance, 
                                         transaction_remark,
                                         transaction_date)
                                         VALUES (@WalletId,
                                         @TransactionAmount,
                                         @TransactionTypeId,
                                         @UpdatedBalance, 
                                         @TransactionRemark,
                                         @TransactionDate)  
                                         RETURNING lastval()";

            var parameters = new DynamicParameters();

            parameters.Add("@WalletId", walletTransaction.WalletId);
            parameters.Add("@TransactionAmount", walletTransaction.TransactionAmount);
            parameters.Add("@TransactionTypeId", walletTransaction.TransactionType);
            parameters.Add("@UpdatedBalance", walletTransaction.UpdatedBalance);
            parameters.Add("@TransactionRemark", walletTransaction.TransactionRemark);
            parameters.Add("@TransactionDate", DateTime.UtcNow);
           
                var result = await _genericRepository.ExecuteScalarAsync<int>(insertToWalletTransactionQuery, parameters).ConfigureAwait(false);
                return result;
           
        }
        public async Task<int> AddUserWallet(int userId)
        {
            var sQuery = @" INSERT INTO public.ohd_user_wallet(  
                            user_id,
                            balance,
                            status_id,
                            created_at)
                            VALUES
                            (
                            @UserId,
                            @MainBalance,
                            @StatusId,
                            @CreatedAt
                            )
                             RETURNING lastval()";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@MainBalance", 0);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdateUserWallet(UpdateWalletBalanceQuery request, double UpdatedBalance)
        {
            var updateUserWalletQuery = @"UPDATE public.ohd_user_wallet SET 
                                    balance=@UpdatedBalance,
                                    status_id=@StatusId,
                                    last_modified_at=@LastModifiedAt
                                    WHERE user_id=@UserId;
                                   Select Id From ohd_user_wallet
                                    WHERE user_id = @UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", request.UserId);
            //parameters.Add("@UserId", request.UserId);
            parameters.Add("@UpdatedBalance", UpdatedBalance);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@LastModifiedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(updateUserWalletQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion

         public async Task<IEnumerable<UserWalletTransactionDto>> GetUserWalletTransaction(GetUserWalletTransactionQuery request)
        {
            try
            {
                var sQuery = @"SELECT wt.txnid AS TransactionId,  wt.transaction_amount AS TransactionAmount
                                            ,tt.transaction_type AS TransactionType 
                                            --,wt.updated_balance AS UpdatedWalletBalance,
                                            ,wt.transaction_remark As TransactionRemark
                                            ,to_char(wt.transaction_date,'dd-MM-yyyy HH24:MI:ss') AS TransactionDate
                                            FROM public.ohd_wallet_transaction AS wt
                                            LEFT JOIN public.ohd_transaction_type_master AS tt ON wt.transaction_type_id=tt.id
                                            LEFT JOIN public.ohd_user_wallet  AS w ON wt.wallet_id=w.id";
                var parameters = new DynamicParameters();
                if (request.UserId > 0)
                {
                    sQuery += " WHERE w.user_id=@UserId";
                    parameters.Add("@UserId", request.UserId);
                }
                sQuery += " order by wt.transaction_date desc";
                var walletTransactions = await _genericRepository.GetAsync<UserWalletTransactionDto>(sQuery, parameters).ConfigureAwait(false);
                return walletTransactions;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }
        public async Task<double> AddBalanceAsync(int userId, double amountToAdd,double walletbalance, string transactionRemark, bool useWallet, double topupAmount,string txnId)
        {
            double walletBalance = Math.Round((walletbalance + amountToAdd), 4);
            string query = @"UPDATE public.ohd_user_wallet 
                                SET balance =@walletBalance,
                                last_modified_at =@ModifiedAt 
                                WHERE user_id = @userId";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@walletBalance", walletBalance);
            parameters.Add("@amountToAdd", amountToAdd);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            await _genericRepository.ExecuteScalarAsync<int>(query, parameters);
            var wallet = await GetWalletAsync(userId).ConfigureAwait(false);
            if (!useWallet)
            {
                await saveTransaction((int)TransactionType.Credit, amountToAdd, transactionRemark, wallet.Id,txnId);
            }
            if (useWallet && topupAmount > walletbalance)
            {
                await saveTransaction((int)TransactionType.Credit, amountToAdd, transactionRemark, wallet.Id,txnId);
            }

            return amountToAdd;
        }

        public async Task CreateWalletAsync(int userId)
        {
            string query = @"insert into public.ohd_user_wallet(userid, balance) 
                             values(@userId, 0)";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);

            await _genericRepository.ExecuteScalarAsync<int>(query, parameters);
        }

        public async Task<double> DeductBalanceAsync(int userId, double amountToDeduct, double walletbalance, string transactionRemark, bool useWallet,string txnId)
        {
            double walletBalance = Math.Round((walletbalance - amountToDeduct), 4);
            string query = @"UPDATE public.ohd_user_wallet 
                            SET balance = @walletBalance , 
                            last_modified_at =@ModifiedAt
                            WHERE user_id = @userId";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@walletBalance", walletBalance);
            parameters.Add("@amountToDebit", amountToDeduct);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);


            await _genericRepository.ExecuteScalarAsync<double>(query, parameters);
            var wallet= await GetWalletAsync(userId).ConfigureAwait(false);
            await saveTransaction((int)TransactionType.Debit, amountToDeduct, transactionRemark, wallet.Id,txnId);

            return amountToDeduct;
        }

        public async Task<double> GetBalanceAsync(int userId)
        {
            try
            {
                string query = @"select balance from public.ohd_user_wallet where user_id = @userId";
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);

                var balance = await _genericRepository.ExecuteScalarAsync<int>(query, parameters);

                return balance;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<Core.Domain.Models.Dto.Wallet.Wallet> GetWalletAsync(int userId)
        {
            string query = @"select * from public.ohd_user_wallet where user_id = @userId";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);

            var wallet = await _genericRepository.GetFirstOrDefaultAsync<Core.Domain.Models.Dto.Wallet.Wallet>(query, parameters);
            return wallet;
        }

        public async Task<double> RefundBalanceAsync(int userId, double totalAmount,string txnId)
        {
            string query = @"UPDATE public.ohd_user_wallet 
                    SET balance = @totalAmount,
                    last_modified_at =@ModifiedAt
                    WHERE user_id = @userId";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@totalAmount", totalAmount);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            await _genericRepository.ExecuteScalarAsync<int>(query, parameters);
            var wallet = await GetWalletAsync(userId).ConfigureAwait(false);
            await saveTransaction((int)TransactionType.Refunded, totalAmount, totalAmount + " refunded to wallet", wallet.Id,txnId);
            

            return totalAmount;
        }

        public bool WalletExists(int userId)
        {
            string query = @"select count(*) from public.ohd_user_wallet where userid = @userId";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);

            var walletCount = _genericRepository.ExecuteScalarAsync<int>(query, parameters).GetAwaiter().GetResult();

            return walletCount > 0;
        }

        public async Task saveTransaction(int transactionType, double transactionAmount, string remark, int walletId,string TxnId)
        {
            string transactionQuery = @"insert into public.ohd_wallet_transaction
                                        (wallet_id,txnid,transaction_type_id ,transaction_amount ,transaction_date,transaction_remark)  
                                        values(@walletId,@TxnId, @transactionType, @transactionAmount, @transactionDate,@transactionRemark)
                                        RETURNING lastval()";

            var parameters = new DynamicParameters();
            parameters.Add("@walletId", walletId);//TODO add walletId to transaction
            parameters.Add("@transactionType", transactionType);
            parameters.Add("@transactionAmount", transactionAmount);
            parameters.Add("@transactionRemark", remark);
            parameters.Add("@TxnId", TxnId);
            parameters.Add("@transactionDate", DateTime.UtcNow);
            try
            {
                await _genericRepository.ExecuteScalarAsync<int>(transactionQuery, parameters);
            }
            catch (Exception ex)
            {
            }
        }
    }


}
