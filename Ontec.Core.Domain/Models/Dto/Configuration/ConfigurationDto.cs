namespace Ontec.Core.Domain.Models.Dto.Configuration
{
    public class ConfigurationDto : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        // public string CreatedAt { get; set; }
        //public int CreatedBy { get; set; }
        //public string ModifiedAt { get; set; }
        //public int ModifiedBy { get; set;}
        public string DisplayName { get; set; }
        public string Note { get; set; }
        public bool isEditable { get; set; }
        public string ConfigurationType { get; set; }

       

    }
}
