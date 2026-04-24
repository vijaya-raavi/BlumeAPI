using MediatR;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateUserCommand : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public int TitleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public string Mobile { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
        public int CompanyId { get; set; }
        public string AddressLatitude { get; set; }
        public string AddressLongitude { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public int StatusId { get; set; }
        public IEnumerable<int> CommunicationTypesIds { get; set; }
        public int ProofDocumentTypeId { get; set; }
        public required IFormFile ProofDocument { get; set; }
        public string? DocumentNumber {  get; set; }
        public string? TaxNumber { get; set; }
       public string Comments {  get; set; }
        public bool TermsAccepted {  get; set; }
        public string TermConditionsVersion { get; set; }
    }
}
