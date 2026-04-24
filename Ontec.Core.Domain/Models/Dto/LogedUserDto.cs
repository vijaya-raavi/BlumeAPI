namespace Ontec.Core.Domain.Models.Dto
{
    public class LogedUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int CompanyId { get; set; }
    }
}
