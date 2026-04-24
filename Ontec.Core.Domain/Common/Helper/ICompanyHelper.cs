using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Common.Helper
{
    public interface ICompanyHelper
    {
        Task<CompanyDetailsDto> GetCompany(int id);
    }
}
