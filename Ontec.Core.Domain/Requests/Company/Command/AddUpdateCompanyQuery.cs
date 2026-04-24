using MediatR;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Company.Command
{
    public class AddUpdateCompanyQuery : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }

        public int StatusId { get; set; }
        public string Description { get; set; }
        public string VAT {  get; set; }
        public string RegistrationNumber { get; set; }
        public List<SocialMediaLinks> MediaUrls {  get; set; }

    }
    public class SocialMediaLinks
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
