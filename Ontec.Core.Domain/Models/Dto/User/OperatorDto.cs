namespace Ontec.Core.Domain.Models.Dto.User
{
    public class OperatorDto
    {
        public int Id { get; set; }
        public string ProfileUrl { get; set; }
        public string Operator { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CreatedOn { get; set; }
        public Boolean Status { get; set; }
        public string Password { get; set; }

        public string FirstName {  get; set; }
        public string LastName { get; set; }
    }
}
