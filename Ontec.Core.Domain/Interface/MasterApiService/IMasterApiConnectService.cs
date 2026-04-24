using Ontec.Core.Domain.Models.Dto.AccountTransactions;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.MasteUserAccount;
using Ontec.Core.Domain.Models.Dto.Transaction;

namespace Ontec.Core.Domain.Interface.MasterApiService
{
    public interface IMasterApiConnectService
    {
        public Task<MeterDataModel> GetMeter(string url);
        public Task<IntervalMeterReadingDataModel> GetMeterReadingIntervals(string url);
        public Task<CustomerAuxAccount> GetCustomerAuxAccount(string url);
        public Task<AccountTransactionApiModel> GetAccountTransactions(string url);
        Task<TransactionMasterApiModel> GetCustomerTransactions(string url);
    }
}
