using Ontec.Core.Domain.Models.Dto.Otp;

namespace Ontec.Core.Domain.Interface.Otp
{
    public interface IOtpRepository
    {
        Task<int> InsertOtp(OtpModel model);
        Task<int> GetCountryCodeId(string code, int statusId);
        Task<OtpModel> GetOtpByMobileNumberCompanyId(string mobile, int companyId,string emailId, int statusId);
        Task<string> GetOtpByMobileNumberCompanyId(string mobile, int companyId, string emailId);
        Task<int> UpdateVerifiedOtp(int id,int statusId, string verifiedKey);
        Task<string> GetVerifiedKey(string mobile, int companyId, string emailId);


    }
}
