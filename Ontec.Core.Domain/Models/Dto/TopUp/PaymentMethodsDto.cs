namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public  class PaymentMethodsDto : BaseModel
    {
        public string  Name  {get;set;}
        public string DisplayName  {get;set;}
        public string Slug { get; set; }
        public double Percentage  { get; set; }
        public double Discount { get; set; }
        public bool StatusId { get; set; } 
        public string LekkaPaySlug {  get; set; }
    }
}
