namespace Ontec.Core.Domain.Models.Dto.User
{
    public class LoginAttemptUserDto : BaseModel
    {
        public string FirstName {  get; set; }
        public string Email {  get; set; }
        public string Mobile {  get; set; }
        public string EmailMobile { get; set; }
        public int FailedCountAttempted { get; set; }
        public DateTime LastAttempted { get; set; }
        public bool IsBlocked { get; set; }
        public int MinutesFromLastFailledattempts{  get; set; }

        public string Password {  get; set; }
    }
}
