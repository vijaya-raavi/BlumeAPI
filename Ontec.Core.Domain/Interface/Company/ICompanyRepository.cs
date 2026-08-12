using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Company;
using Ontec.Core.Domain.Requests.Company.Command;
using Ontec.Core.Domain.Requests.Company.Queries;

namespace Ontec.Core.Domain.Interface.Company
{
    public interface ICompanyRepository
    {
        Task<bool> IsCompanyExist(int companyId);
        Task<int> UpdateCompany(AddUpdateCompanyQuery request);
        Task<int> AddCompany(AddUpdateCompanyQuery request);
        Task<bool> IsCountryExist(int countryId);
        Task<bool> IsStateExist(int stateId);
        Task<IEnumerable<OntecSelectListItem>> GetCountries();
        Task<IEnumerable<OntecSelectListItem>> GetStates();
        Task<CompanyDto> GetCompanyDetails(int id);
        Task<int> UpdateCompanyLogo(string CompanyLogoUrl, int id, int Type);
        Task<IEnumerable<OntecSelectListItem>> GetPaymentGateWays();
    }
}
