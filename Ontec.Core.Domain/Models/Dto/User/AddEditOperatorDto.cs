namespace Ontec.Core.Domain.Models.Dto.User
{
    public class AddEditOperatorDto : BaseModel
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public int CompanyId { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public int StatusId { get; set; }

    }
}
