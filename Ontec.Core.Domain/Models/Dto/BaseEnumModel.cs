namespace Ontec.Core.Domain.Models.Dto
{
    public class BaseEnumModel : BaseModel
    {
        public string Name { get; set; }
        public string DisplayValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
