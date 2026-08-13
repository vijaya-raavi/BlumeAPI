using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ontec.Core.Domain.Models.Dto.AccountTransactions;

namespace Ontec.Core.Domain.Models.Dto.MasteUserAccount
{
    public class AuxAccountListResponse
    {
        public List<AuxAccountDto> Data { get; set; }


    }

    public class AuxAccountDto
    {
        public string Id { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public decimal? PrincipleAmount { get; set; }
        public int AccountPriority { get; set; }
        public int AuxChargeScheduleId { get; set; }
        public string RecordStatus { get; set; }
        public DateTime? SuspendUntil { get; set; }
        public DateTime StartDate { get; set; }
        public ChargeScheduleData ScheduleCharges  { get; set; }
        public List<AccountTransaction> Transactions { get; set; }
    }
    public class ChargeScheduleData
    {
        public List<ChargeScheduleDto> Data { get; set; }
    }

    public class ChargeScheduleDto
    {
        public decimal VendPortion { get; set; }
        public decimal ChargeAmt { get; set; }
        public decimal CurrentPortion { get; set; }
        public decimal DailyAmount { get; set; }
        public decimal MinAmt { get; set; }
        public decimal MaxAmt { get; set; }
        public string ChargeCycle { get; set; }

        public string RecordStatus { get; set; }
        public string ScheduleName { get; set; }
        public string AccountSpecific { get; set; }
    }
    public class AuxTransactionDto
    {
        public string Id { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
    }




}

