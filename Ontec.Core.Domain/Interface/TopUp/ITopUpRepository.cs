using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.AdminDashboard;
using Ontec.Core.Domain.Models.Dto.Debitech;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Requests.Dashboard.Command;
using Ontec.Core.Domain.Requests.Debitech.Command;
using Ontec.Core.Domain.Requests.Debitech.Queries;
using Ontec.Core.Domain.Requests.TopUp.Command;
using Ontec.Core.Domain.Requests.TopUp.Queries;

namespace Ontec.Core.Domain.Interface.TopUp
{
    public interface ITopUpRepository
    {
        #region TopupTransactions
        Task<int> IsTransactionNoExist(string transactionId);
        Task<int> AddTopupTransactions(AddTopUpTransactionQuery request,
                                                string transactionId,
                                                double transactionFee,
                                                double topUpAmount,
                                                double WalletAmountUsed,
                                                double FinalAmountToPay,
                                                string currentPaymentGateWay);
        Task<int> UpdateTopupTransactions(PayFastModel request);
        Task<AddTopUpTransactionsDto> GetTopUpTransaction(int Id);
        Task<GetTopUpTransaction> GetTopupTransactionDetails(string transactionNo);
        Task<DatatableModel<GetTopUpTransaction>> GetTopupTransactions(GetTopUpTransactionsQuery request);
        Task CancelTopUpTransaction(CancelTopUpTransactionQuery request);
        Task UpdateTransactionStatus(string status,int flag, string data, int id, string token,string keyChangeToken,
                                    string bsstToken, string mrktMsg ,string customerMsg, string RctNum,string tarrif, string VendReference);
        Task UpdateTrailTransactionStatus(string status,int flag, string data, int id,double debt);
        Task<IEnumerable<PaymentMethodsDto>> GetDebitechPaymentMethods(GetPaymentMethodsQuery request);
        #endregion

        #region BankAccount
        Task<int> IsBankAccountExist(string accountNumber, int Id);

        Task<bool> IsBankAccountIdExist(int id);
        Task<int> AddBankAccount(AddOrUpdateBankAccountQuery request);
        Task<int> UpdateBankAccount(AddOrUpdateBankAccountQuery request);
        Task DeleteBankAccountById(int Id);
        Task<IEnumerable<BankAccountDto>> GetBankAccounts(GetBankAccountsQuery request);
        Task<DatatableModel<UserPaymentsDto>> GetUserPayments(GetUserPayamenstQuery request);

        #endregion
        #region PaymentMethods
        Task<IEnumerable<PaymentMethodsDto>> GetPaymentMethods(GetPaymentMethodsQuery request);
        Task UpdatePaymentMethods(UpdatePaymentMethodsQuery request, int UserId);
        #endregion
        Task<int> AddBankTransferTransaction(AddBankTransferTransactionDto request);
        Task<int> IsNetUpTransactionGuidExist(string transactionReferenceNo);
        Task<int> IsBankTransactionIdExist(string bankTransactionId);
        Task<PaymentDashboardDto> GetPaymentDashboard();
        Task<AdminDashboardDto> GetAdminDashboard();
        Task<PurchaseReceiptDto> GetReceiptDetails(DownloadPurchaceRecieptPdfQuery request);
        Task<PurchaceReceiptResponseModel> DownloadPurchaceRecieptPdfQuery(DownloadPurchaceRecieptPdfQuery request,int userId);
        Task<IEnumerable<PaymenthMethodSummary>> GetPaymenthMethodSummaries();
        Task<string> GetTransactionNoFromRctNum(string rctNum);
        Task<double> GetTransactionNoFeeFromRctNum(string rctNum);
        Task<int> ISVendResponseExist(string vendResponse);
        Task<IEnumerable<PaymentMethodsDto>> GetAllPaymentMethods();
        Task<int> ISRCTNoExist(string RctNo);
        Task<int> AddTopupTransactionsFromSTSResponse(AddSTSTopUpHelper helper);
        Task<GetSTSTopUpTransactions> GetSTSTopUps(List<string> RecNumn,DateTime fromdate,DateTime toDate);
        Task<string> GetIPayMethodByPaymentMethodId(int id);
        Task<int> UpdateTopupTransactionFee(double rechargeAmt, double txnFee, string TxnId);
        Task<int> UpdateWalletTopupTransactions(WalletTopUpModel request);
        Task UpdateTransactionUseWallet(int id);
        Task<int> AddDebitechFailedTxn(AddDeitecFailedTransactionDto request);
        Task<int> SaveDebitechgetNetChecksumRequest(GetNetCheckSumQuery request, string checksum);
        Task<int> SaveDebitechNotifyRequest(BankNotificationRequestQuery request);
        Task<int> UpdateDebitechNotifyResponse(List<BankNotificationResponseDto> response, int debitechNotifyId);
        Task<Dictionary<string, TransactionNoFeeFromRctNumDto>> GetTransactionFeesByReceiptNumbers(List<string> receiptNumbers);
        Task DeleteDebitechDuplicateNotifyRequest(int id);
        Task<int> SaveCredits(CaptureUsersCreditsToSaveCommandReuqest request, string imagePath, int imgCount);
        Task<int> UpdateCreditImages(string imagePath, string columnName, int imageCount, int id);
        Task<CreditImageDto> GetCreditsId(int meterId, int userId);
        Task<CreditImagesRawDto> GetCreditImageValues(int meterId, int userId);
        Task<int> UpdateAllImages(long id, string img1, string img2, string img3, string img4, string img5, string img6, int count);
        Task<int> SaveCreditImage(CaptureUsersCreditsToSaveCommandReuqest reuqest, string imageUrl);
        Task<List<UserCreditImageDto>> GetUserImages(long meterId);
        Task<DatatableModel<GetTopUpTransaction>> GetTrailVendSuccessTopupTransactions(GetUserPayamenstQuery request);
    }
}
