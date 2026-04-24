using Microsoft.Extensions.Logging;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Models.Dto.AccountTransactions;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.MasteUserAccount;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.MasterApi.GenericApi;

namespace Ontec.Infrastructure.Persistence.Repositories.MasterApiService
{
    public class MasterApiConnectService : GenericApiService, IMasterApiConnectService
    {
        public MasterApiConnectService(ILogger<MasterApiConnectService> logger) : base(logger)
        {

        }


        public async Task<MeterDataModel> GetMeter(string url)
        {
            var result = await MasterGetAsync<string, MeterDataModel>(url);
            return result;
        }

        public async Task<IntervalMeterReadingDataModel> GetMeterReadingIntervals(string url)
        {
            var result = await MasterGetAsync<string, IntervalMeterReadingDataModel>(url);
            return result;
        }

        public async Task<CustomerAuxAccount> GetCustomerAuxAccount(string url)
        {
            var result = await MasterGetAsync<string, CustomerAuxAccount>(url);
            return result;
        }

        public async Task<AccountTransactionApiModel> GetAccountTransactions(string url)
        {
            var result = await MasterGetAsync<string, AccountTransactionApiModel>(url);
            return result;
        }
        public async Task<TransactionMasterApiModel> GetCustomerTransactions(string url)
        {
            var result = await MasterGetAsync<string, TransactionMasterApiModel>(url);
            return result;
        }
    }
    
}
