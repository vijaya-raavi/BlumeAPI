namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class BankAccountDto : BaseModel
    {
        public string BankName {  get; set; }
        public string  AccountName { get; set; }
        public string AccountNumber {  get; set; }
        public string BranchCode { get; set; }
        public string CreatedAt { get; set; }
        public string Status { get; set; }
        public string ModifiedAt {  get; set; }
    }
}
