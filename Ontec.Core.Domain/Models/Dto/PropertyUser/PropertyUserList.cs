using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models.Dto.PropertyUser
{
    public class PropertyUserDto
    {
        public string PropertyName { get; set; }
        public string RoleForProperty { get; set; }
        public PropertyUserList Owner { get; set; }
        public PropertyUserList Tenant { get; set; }
        public List<PropertyUserList> Associates { get; set; }
    }
    public class PropertyUserList : BaseModel
    {
        public string ProfileUrl { get; set; }
        public DocumentResultDto Profile {  get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string CreatedOn { get; set; }
        public string RoleName { get; set; }
        public bool TopUpAllow { get; set; }
        public int SystemUserId { get; set; }
    }
}
