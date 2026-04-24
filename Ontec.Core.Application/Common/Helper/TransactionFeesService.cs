using Dapper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.AdminDashboard;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.Wallet.Command;

namespace Ontec.Core.Application.Common.Helper
{
    public interface ITransactionFeesService
    {
        Task<TransactionFeeDto> CalculateTransactionFee(int paymentType, double amount,bool UseWallet, int UserId);
    }
    public class TransactionFeesCalculation : ITransactionFeesService
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IWalletRepository _walletRepository;
        public TransactionFeesCalculation(IGenericRepository genericRepository,IWalletRepository walletRepository)
        {
            _genericRepository = genericRepository;
            _walletRepository = walletRepository;
        }

        public async Task<TransactionFeeDto> CalculateTransactionFee(int paymentType, double amount,bool UseWallet,int UserId)
        {
            double transactionFees = 0;
            double rechargeAmount = 0;
            double finalAmountToPay = 0;
            TransactionFeeDto feeDto = new TransactionFeeDto();

            try
            {
                var TransactionFeeQuery = @"SELECT percentage FROM public.ohd_payment_methods where id=@Id;";
                var DiscountQuery = @"SELECT discount FROM public.ohd_payment_methods where id=@Id;";
                var GetWalletBalance = @"SELECT balance FROM public.ohd_user_wallet WHERE user_id=@UserId;";

                var parameters = new DynamicParameters();
                parameters.Add("@Id", paymentType);
                parameters.Add("@UserId", UserId);
                
                var TransactionFeeTask = _genericRepository.ExecuteScalarAsync<double>(TransactionFeeQuery,parameters);
                var DiscountTask = _genericRepository.ExecuteScalarAsync<double>(DiscountQuery,parameters);
                var WalletBalanceTask = _genericRepository.ExecuteScalarAsync<double>(GetWalletBalance, parameters);


                await Task.WhenAll(TransactionFeeTask, DiscountTask,WalletBalanceTask).ConfigureAwait(false);

                double TransactionFee = TransactionFeeTask.Result;
                double Discount = DiscountTask.Result;
                double WalletAmount=WalletBalanceTask.Result;

                double discount = Discount / 100;
                double transactionfee = TransactionFee / 100;

                
                double amountForTransactionFee = amount;
                if (UseWallet && WalletAmount < amount)
                {
                    amountForTransactionFee=amount-WalletAmount;
                    finalAmountToPay = amount - WalletAmount;

                }
                if (UseWallet && WalletAmount >= amount)
                {
                    amountForTransactionFee = 0;
                }

                if (discount <= transactionfee && amountForTransactionFee >0)
                {
                    transactionFees = amountForTransactionFee * (transactionfee - discount);
                    transactionFees = Math.Round(transactionFees, 2);
                    rechargeAmount = amountForTransactionFee - transactionFees;
                }
                if (discount >= transactionfee && amountForTransactionFee > 0)
                {
                    transactionFees = amountForTransactionFee * (transactionfee - discount);
                    transactionFees = Math.Round(transactionFees, 2);
                    rechargeAmount = amountForTransactionFee + transactionFees;
                }
                if (UseWallet && WalletAmount < amount)
                {
                    rechargeAmount += WalletAmount;
                    feeDto.WalletAmountUsed = WalletAmount;
                }
                if (UseWallet && WalletAmount >= amount)
                {
                    rechargeAmount += amount;
                    feeDto.WalletAmountUsed = amount;
                    transactionFees = 0;
                }

                feeDto.DiscountFee= discount;
                feeDto.TransactionFee = transactionFees;
                feeDto.TopUpAmount = rechargeAmount;
                feeDto.FinalAmountToPay = finalAmountToPay;


            }
            catch (Exception ex)
            {
                throw;
            }
            return feeDto; 
        }
        private double UpdateWallet(int userId,double amount,double WalletAmount)
        {
            UpdateWalletBalanceQuery updateWalletBalance = new UpdateWalletBalanceQuery
            {
                UserId = userId,
                Amount = amount
            };
            double UpdatedBalance = WalletAmount + updateWalletBalance.Amount;
            _walletRepository.UpdateUserWallet(updateWalletBalance, UpdatedBalance);
            return UpdatedBalance;
        }
    }

    
}

