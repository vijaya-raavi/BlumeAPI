using MediatR;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class BulkUploadRequestUsersCommand : IRequest<AddUpdateResultDto>
    {
        public IEnumerable<BulkUsers> UploadBulkUsers { get; set; }
    }

    public class BulkUsers
    {
        public int Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
        public int CompanyId { get; set; }
        public string AddressLine1 { get; set; }
        public int CountryCodeId { get; set; }
        public IEnumerable<int> CommunicationTypesIds { get; set; }
        public string? TaxNumber { get; set; }
        public bool TermsAccepted { get; set; }
        public string TermConditionsVersion { get; set; }

        public BulkUserDocument? UserDocument { get; set; }
        public List<PropertyRequestDto> UserProperties { get; set; }

    }

    public class PropertyRequestDto
    {
        public string PropertyName { get; set; }
        public string UnitNumber { get; set; }
        public int EstateId { get; set; }
        public string AddressLine1 { get; set; }
        public List<PropertyMeterRequestDto> Meters { get; set; } = new List<PropertyMeterRequestDto>();

    }

    public class PropertyMeterRequestDto
    {
        public int MeterTypeId { get; set; }
        public string MeterNumber { get; set; }
        public string Alias { get; set; }
        public double TargetConsumption { get; set; }
        public string? ContractEndDate { get; set; }
        public IFormFile? ContractDocumentUrl { get; set; }
    }
    public class BulkUserDocument

    {
        public string DocumentType { get; set; }
        public string DocNumber { get; set; }
        public IFormFile FileDoc { get; set; }
    }
}
