namespace Ontec.Core.Domain.Models.Dto.MasteUserAccount
{
    public class CustomerAuxAccount
    {
        public List<AuxAccount> Data { get; set; }
    }
    public class AuxAccount
    {
        public string Id { get; set; }
        public bool IdExternal { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public int AccountPriority { get; set; }
        public string CustomerAgreementId { get; set; }
        public string AuxTypeId { get; set; }
        public string RecordStatus { get; set; }
        public string AuxChargeScheduleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SuspendUntil { get; set; }
    }
}
