namespace Ontec.Core.Domain.Models.Dto.User
{
    public class LoggedUserDto
    {
        public int UserId { get; set; }
        public string Role { get; set; }
        public string Mobile { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string LastLoginDate {  get; set; }
    }
}
