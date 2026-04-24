namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class GetUserCardsDto :BaseModel
    {

        public string CardNumber { get; set; }
        public string Name { get; set; }
        public string Expiry { get; set; }
        public Boolean Default { get; set; }
        public string  Status { get; set; }
    }
}
