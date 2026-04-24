namespace Ontec.Core.Domain.Models.Dto.Consumption
{
    public class CustomerDto : BaseMeterConsumptionModel
    {
        public string Surname { get; set; }
        public string RecordStatus { get; set; }
        public string CustomVarchar1 { get; set; }
        public string CustomerReference { get; set; }
    }
    public class CustomerAccountDto : BaseMeterConsumptionModel
    {
        public string CustomerId { get; set; }
        public string AccountName { get; set; }
        public double AccountBalance { get; set; }
        public double CreditLimit { get; set; }
        public string RecordStatus { get; set; }
        public string NotificationEmail { get; set; }
        public string NotificationPhone { get; set; }
        public bool NotifyLowBalance { get; set; }
    }
    public class CustomerAgreementDto : BaseMeterConsumptionModel
    {
        public string AgreementRef { get; set;}
        public string RecordStatus { get; set; }
        public bool Billable { get; set; }
    }


}
