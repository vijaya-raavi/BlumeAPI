namespace Ontec.Core.Domain.Models.Dto.Login
{
    public class LoginAttemptsDto 
    {
        public int Id { get; set; }
        public int UserId {  get; set; }
        public int AttemptsCount { get; set; }
        public string CreatedAt { get; set; }
    }
}
