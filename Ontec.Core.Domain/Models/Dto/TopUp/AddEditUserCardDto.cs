namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public  class AddEditUserCardDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ValidThrough { get; set; }
        public Boolean UseAsDefault { get; set; }
        public string MaskedCardNumber {  get; set; }
        public int? StatusId { get; set; }
    }
}
