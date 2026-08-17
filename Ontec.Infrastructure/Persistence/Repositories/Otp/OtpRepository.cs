using System.Web.Helpers;
using Dapper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Models.Dto.Otp;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Ontec.Infrastructure.Persistence.Repositories.Otp
{
    public class OtpRepository : IOtpRepository
    {
        private readonly IGenericRepository _genericRepository;
        public OtpRepository(IGenericRepository genericRepository)
        {
            _genericRepository = genericRepository;
        }
        public async Task<int> InsertOtp(OtpModel model)
        {
            var sQuery = @"INSERT INTO public.ohd_otp(
	                    ""Otp"", ""Email"", ""StatusId"", created_at, modified_at, ""CountryCodeId"", ""companyId"", ""MobileNumber"")
                               VALUES (@otp 
                                    ,@email
                                    ,@statusId
                                    ,@created_at
                                    ,@modified_at
                                    ,@CountryCodeId 
                                    ,@companyId
                                    ,@mobileNumber )
                        RETURNING lastval()";

            var parameters = new DynamicParameters();
            parameters.Add("@otp", model.Otp);
            parameters.Add("@mobileNumber", model.MobileNumber);
            parameters.Add("@email", model.Email.ToLower());
            parameters.Add("@statusId", model.StatusId);
            parameters.Add("@companyId", model.CompanyId);
            parameters.Add("@created_at", model.CreatedAt);
            parameters.Add("@modified_at", model.ModifiedAt);
            parameters.Add("@CountryCodeId", model.CountryCodeId);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> GetCountryCodeId(string code, int statusId)
        {
            var sQuery = @"SELECT Id FROM ohd_mobile_country_code 
                           WHERE code=@code and status_id=@statusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@code", code);
            parameters.Add("@statusId", statusId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<OtpModel> GetOtpByMobileNumberCompanyId(string mobile, int companyId, string emailId, int statusId)
        {
            var sQuery = @"SELECT ""Otp"" as Otp,""Id"" as Id,
                            created_at as CreatedAt 
                            FROM public.ohd_otp
                            WHERE  ""companyId""=@companyId 
                            AND ""MobileNumber""=@mobileNumber 
                            AND (""StatusId""=@statusId  OR ""StatusId""=@Pending)
                            AND Lower(""Email"")=@emailId";

            var parameters = new DynamicParameters();
            parameters.Add("@companyId", companyId);
            parameters.Add("@mobileNumber", mobile);
            parameters.Add("@statusId", statusId);
            parameters.Add("@Pending", (int)StatusEnum.Pending);
            parameters.Add("@emailId", emailId.ToLower());

            var result = await _genericRepository.GetFirstOrDefaultAsync<OtpModel>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<string> GetOtpByMobileNumberCompanyId(string mobile, int companyId, string emailId)
        {
            var sQuery = @"Select ""Otp"" as Otp,""Id"" as Id, created_at as CreatedAt from public.ohd_otp
                          where  ""companyId""=@companyId and ""MobileNumber""=@mobileNumber and  Lower(""Email"")=@emailId";

            var parameters = new DynamicParameters();
            parameters.Add("@companyId", companyId);
            parameters.Add("@mobileNumber", mobile);

            parameters.Add("@emailId", emailId.ToLower());

            var result = await _genericRepository.GetFirstOrDefaultAsync<string>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> UpdateVerifiedOtp(int id, int statusId, string verifiedKey)
        {
            var sQuery = @"Update public.ohd_otp
                            Set ""StatusId""=@statusId,
                            verified_key=@VerifiedKey
                          where  ""Id""=@Id;
                          Select ""Id"" from public.ohd_otp
                          where  ""Id""=@Id  ";

            var parameters = new DynamicParameters();
            parameters.Add("@statusId", statusId);
            parameters.Add("@VerifiedKey", verifiedKey);
            parameters.Add("@Id", id);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<string> GetVerifiedKey(string mobile, int companyId, string emailId)
        {

            var sQuery = @"SELECT verified_key FROM public.ohd_otp
                           where  ""companyId""=@companyId   AND ""StatusId""=@statusId
                            ";

            var parameters = new DynamicParameters();
            parameters.Add("@companyId", companyId);
            if (!string.IsNullOrEmpty(mobile))
            {
                sQuery+= @" AND ""MobileNumber""= @mobileNumber" ;
                parameters.Add("@mobileNumber", mobile);
            }
            if (!string.IsNullOrEmpty(emailId))
            {
                sQuery += @" and Lower(""Email"")= @emailId ";

                parameters.Add("@emailId", emailId.ToLower());
            }
            sQuery += @" order by ""Id"" desc limit 1";


            parameters.Add("@statusId", (int)StatusEnum.Pending);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<string>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
