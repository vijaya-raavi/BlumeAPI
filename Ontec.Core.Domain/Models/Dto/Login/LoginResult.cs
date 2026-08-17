using Microsoft.VisualBasic;
using Ontec.Core.Domain.Models.Dto.User;

namespace Ontec.Core.Domain.Models.Dto.Login
{
    public class LoginResult : LoggedUserDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; }
        public Boolean IsBlocked { get; set; }
        public bool IsUserValid { get; set; } = false;
        public bool IsProfileComplete { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public int FailedCountAttempted {  get; set; }
        public DateTime? LastAttempted {  get; set; }

        public string Status {  get; set; }
        public string Comments {  get; set; }

        public string FirstName {  get; set; }
        public string LastName {  get; set; }

        public string sessionKey {  get; set; }
        public bool IsBusiness {  get; set; }
       public string AcceptedTermConditionVersion {  get; set; }
        public string IsEstateEnable { get; set; }
        public string Auxaccountdetails { get; set; }
        public bool IsForcedPasswordChange {  get; set; }

        public int CompanyId {  get; set; }
        public bool IsBulkUser { get; set; }
        public int CountryId { get; set; }
        public string CountryCode { get; set; }

    }
}
