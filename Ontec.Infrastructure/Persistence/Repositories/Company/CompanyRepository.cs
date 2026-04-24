using Dapper;
using Newtonsoft.Json;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Company;
using Ontec.Core.Domain.Requests.Company.Command;

namespace Ontec.Infrastructure.Persistence.Repositories.Company
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IGenericRepository _genericRepository;
        public CompanyRepository(IGenericRepository genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public Task<int> AddCompany(AddUpdateCompanyQuery request)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsCompanyExist(int companyId)
        {
            var sQuery = @"SELECT count(id) 
                        FROM public.ohd_company
                          WHERE Id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", companyId);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<int> UpdateCompanyLogo(string CompanyLogoUrl, int id,int Type )
        {
            var sQuery = @"UPDATE ohd_company SET
                             modified_at = @ModifiedAt";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            if (Type == (int)ImageType.Regular)
            {
                sQuery += @", companylogourl = @CompanyLogoUrl
                                WHERE id = @Id;
                                SELECT id from ohd_company WHERE id=@Id;";
;
                parameters.Add("@CompanyLogoUrl", CompanyLogoUrl);
            }
            if (Type == (int)ImageType.Small)
            {
                sQuery += @", smalllogourl = @SmallLogourl
                                WHERE id = @Id;
                                SELECT id from ohd_company WHERE id=@Id;";
                parameters.Add("@SmallLogourl", CompanyLogoUrl);
            }
            if (Type == (int)ImageType.Favicon)
            {
                sQuery += @", faviconlogourl = @faviconlogourl
                                WHERE id = @Id;
                                SELECT id from ohd_company WHERE id=@Id;";
                parameters.Add("@faviconlogourl", CompanyLogoUrl);
            }
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<bool> IsCountryExist(int countryId)
        {
            var sQuery = @"SELECT count(id) 
                          FROM public.ohd_country
                          WHERE Id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", countryId);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<bool> IsStateExist(int stateId)
        {
            var sQuery = @"SELECT count(id) 
                           FROM public.ohd_state
                          WHERE Id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", stateId);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<int> UpdateCompany(AddUpdateCompanyQuery request)
        {
            // Serialize the updated list back to JSON

           string updateMediaUrl= JsonConvert.SerializeObject(request.MediaUrls);

            try
            {
                var sQuery = @" Update ohd_company  set name=@Name
                                                   --,domain_name=@DomainName 
                                                  ,status_id=@Status_id
                                                  ,firstname=@FirstName
                                                  ,lastname=@LastName
                                                  ,address=@Address
                                                  ,country_id=@Country
                                                  ,state_id=@State
                                                  ,city=@City
                                                 ,mobile=@Mobile
                                                  ,email=@Email
                                                  ,zip=@Zip
                                                  ,description=@Description
                                                  ,modified_at=@modified_at 
                                                    ,social_media_links=@SocialMediaLinks
                                                    ,vat_no=@VAT
                                                    ,registration_number=@RegistrationNumber
                                                    WHERE id = @Id ;
                                                  Select Id From ohd_company WHERE id = @Id";

                var parameters = new DynamicParameters();
                parameters.Add("@Name", request.CompanyName);
                parameters.Add("@Status_id", request.StatusId);
                parameters.Add("@FirstName", request.FirstName);
                parameters.Add("@LastName", request.LastName);
                parameters.Add("@Address", request.Address);
                parameters.Add("@Country", request.CountryId);
                parameters.Add("@State", request.StateId);
                parameters.Add("@City", request.City);
                parameters.Add("@Mobile", request.MobileNumber);
                parameters.Add("@Email", request.EmailId);
                parameters.Add("@Zip", request.Zip);
                parameters.Add("@Description", request.Description);
                parameters.Add("@modified_at", DateTime.UtcNow);
                parameters.Add("@VAT", request.VAT);
                parameters.Add("@SocialMediaLinks", updateMediaUrl);
                parameters.Add("@RegistrationNumber",request.RegistrationNumber);

                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }
        }
        public async Task<CompanyDto> GetCompanyDetails(int id)
        {
            
            var sQuery = @"SELECT  comp.name As CompanyName
                            ,comp.firstname AS FirstName
                            ,comp.lastname AS LastName
                            ,comp.address AS Address
                            ,comp.country_id As CountryId
                            ,comp.state_id AS StateId
                            ,comp.mobile AS MobileNumber
                            ,comp.email AS EmailId
                            ,comp.city AS City
                            ,comp.zip AS Zip
                            ,comp.description AS Description
                            ,comp.companylogourl AS CompanyLogoUrl
                            ,conf.value AS BackGroundImageUrl
                            ,comp.social_media_links as SocialMediaLinks
                            ,c.name AS CountryName
                            ,s.name AS StateName
                            ,comp.vat_no AS VAT
                            ,comp.registration_number As RegistrationNumber
                            ,con.version AS TermsConditionsVersion
                            ,conf.id As BackgroundId
                            FROM public.ohd_company AS comp 
                            LEFT JOIN public.ohd_configuration AS conf ON comp.id=conf.company_id
                            LEFT JOIN public.ohd_country AS c ON comp.country_id=c.id
							LEFT JOIN public.ohd_state AS s ON comp.state_id =s.id	
                            LEFT JOIN public.ohd_content AS con ON comp.id=con.company_id
                            WHERE  comp.id=@companyId AND conf.name='backgroundImage'
                            AND con.content_name='terms_conditions'";
                var parameters = new DynamicParameters();
                parameters.Add("@companyId", id);

            return await _genericRepository.GetFirstOrDefaultAsync<CompanyDto>(sQuery, parameters);

        }

        public async Task<IEnumerable<OntecSelectListItem>> GetCountries()
        {
            var sQuery = @"	SELECT Id, name
                            FROM ohd_country
                            Where status_id=@StatusId
	                        ORDER BY name ASC  ";
            var parameter = new DynamicParameters();
            parameter.Add("@StatusId", (int)StatusEnum.Active);
            var result = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameter).ConfigureAwait(false);
            return result;
        }
        public async Task<IEnumerable<OntecSelectListItem>> GetStates()
        {
            var sQuery = @"SELECT id,name FROM public.ohd_state
                          Where status_id=@StatusId
                          ORDER BY name ASC ";
            var parameter = new DynamicParameters();
            parameter.Add("@StatusId", (int)StatusEnum.Active);
            var result = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameter).ConfigureAwait(false);
            return result;
        }
       

    }
}

