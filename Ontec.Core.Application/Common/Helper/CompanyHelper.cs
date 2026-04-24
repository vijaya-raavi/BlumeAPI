using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Transaction;

namespace Ontec.Core.Application.Common.Helper
{

    public class CompanyHelper : ICompanyHelper
    {
        private IHostingEnvironment Environment;
        private readonly ICompanyRepository _companyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CompanyHelper(ICompanyRepository companyRepository, IHostingEnvironment _environment,
                       IHttpContextAccessor httpContextAccessor)
        {
            _companyRepository = companyRepository;
            Environment = _environment;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<CompanyDetailsDto> GetCompany(int id)
        {
            var requestPath = _httpContextAccessor.HttpContext.Request;
            var companyDetailsDto = new CompanyDetailsDto();
            var company = await _companyRepository.GetCompanyDetails(id).ConfigureAwait(false);
            string wwwPath = this.Environment.WebRootPath;
            var domain = $"{requestPath.Scheme}://{requestPath.Host}";
            companyDetailsDto.RelativeUrl = domain + company.CompanyLogoUrl;
            string physicalPath = companyDetailsDto.RelativeUrl.Replace(domain, wwwPath).Replace("/", "\\");//uri.LocalPath;
           // companyDetailsDto.RelativeUrl = companyDetailsDto.RelativeUrl.Replace(domain, "http://104.251.223.167:7500/");
            companyDetailsDto.PhysicalUrl = physicalPath;
            companyDetailsDto.Domain = domain;
            companyDetailsDto.Name = company.CompanyName;
            companyDetailsDto.WWWPath = wwwPath;
            companyDetailsDto.Email = company.EmailId;
            return companyDetailsDto;
        }

    }
}
