using MediatR;
using Ontec.Core.Domain.Models.Dto.Company;

namespace Ontec.Core.Domain.Requests.Company.Queries
{
    public class GetCompanyDetailsQuery : IRequest<CompanyDto>
    {
      public int Id { get; set; }

       
    }
}
