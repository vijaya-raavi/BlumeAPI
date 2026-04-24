using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ontec.Core.Domain.Requests.Company.Command
{
    public class UpdateCompanyLogo: IRequest<string>
    {
        public int Id { get; set; }

        public int Type { get; set; }
        public required IFormFile CompanyLogo { get; set; }
    }
}
