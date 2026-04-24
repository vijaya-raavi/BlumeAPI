using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.Company
{
    public class CompanyDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string Description { get; set; }
        public string CompanyLogoUrl { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string BackGroundImageUrl { get; set; }
        public string SocialMediaLinks { get; set; }
        public string TermsConditionsVersion { get; set; }
        public string VAT { get; set; }
        public string RegistrationNumber { get; set; }
        public int BackgroundId { get; set; }

        public bool IsSTSEnable { get; set; }
        public bool IsPropertyTopUp { get; set; }
        public bool EnableTrailVendOnBankTransfer { get; set; }

        public bool IsBankTransferEnable { get; set; }
        public bool IsLekkaPay { get; set; }
    }
}
