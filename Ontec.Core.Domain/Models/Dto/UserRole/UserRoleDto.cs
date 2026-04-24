namespace Ontec.Core.Domain.Models.Dto.UserRole
{
    public class UserRoleDto:BaseModel
    {
        public string Name { get; set; }
        public string Decription { get; set; }
        public int StatusId { get; set; }   
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set;}
    }
}
