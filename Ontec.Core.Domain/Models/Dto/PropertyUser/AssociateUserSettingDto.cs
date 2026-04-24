namespace Ontec.Core.Domain.Models.Dto.PropertyUser
{
    public  class AssociateUserSettingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsAllowTopup { get; set; }
        public int PropertyId {  get; set; }
        public string CreatedAt {get;set; }
        public string ModifiedAt { get; set; }
    }
}
