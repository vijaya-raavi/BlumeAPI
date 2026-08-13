using Ontec.Core.Domain.Models.Dto.CommunicationSetting;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models.Dto.User
{
    public class UserProfileDto : BaseModel
    {
        public string Title { get; set; }
        public int TitleId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public string Mobile { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
        public int CompanyId { get; set; }
        public string AddressLattitude { get; set; }
        public string AddressLongitude { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public IEnumerable<CommunicationType> CommunicationTypes { get; set; }
        public IEnumerable<int> CommunicationTypesIds { get; set; }
        public int ProofDocumentId { get; set; }
        public string ProfileUrl { get; set; }
        public int ProofDocumentTypeId { get; set; }
        public string ProofDocumentUrl { get; set; }
        public string ProofDocumentType { get; set; }

        public DocumentResultDto DocProof { get; set; }
        public DocumentResultDto ProfileImage { get; set; }
        public string DocumentNumber { get; set; }
        public string AddedOn { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string Comments { get; set; }

        public string TaxNumber { get; set; }
        public bool IsBusiness { get; set; }
        public int AllowTopUp { get; set; }
        public bool IsVerified { get; set; }
        public string AcceptedTermConditionVersion { get; set; }
        public string TermsConditionsCurrentVersion { get; set; }
        public string IsWallet { get; set; }
        public string IsEstateEnable { get;set;}

        public string Auxaccountdetails { get; set; }
        public bool IsForcedPasswordChange {  get; set; }
        public bool IsBulkUser { get; set; }
    }
}
