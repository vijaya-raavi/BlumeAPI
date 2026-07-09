namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class UserPaymentsDto
    { 
        public string Consumer {  get; set; }
        public string Email { get; set; }
        public string Phone {  get; set; }
        public string MeterNumber {  get; set; }
        public string TopUp { get; set; }
        public string Mode {  get; set; }
        public string Date {  get; set; }
        public string PropertyRelation { get; set; }
        public string MeterType {  get; set; }

        public string UnitNumber { get; set; }
        public string Property { get; set; }
        public string TopupStatus { get; set; }
        public string Estate {  get; set; }
        public bool IsInHouseTransaction { get; set; }
        public string EFTRefNo { get; set; }
    }
    public class AdminTopUpDto
    {
        public DatatableModel<UserPaymentsDto> UserPayments { get; set; }
        public DatatableModel<GetTopUpTransaction> UserPendingPayments { get; set; }



    }
}
