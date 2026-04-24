using Ontec.Core.Domain.Models.Dto.Otp;

namespace Ontec.Core.Domain.Interface.Common
{
    public interface IWorkContext
    {
        int CurrentUserId { get; }
        int CurrentRoleId { get; }
        string ConnectionId { get; }
        int StatusId { get; }

        int CurrentCompanyId {  get; }
        void SetCurrentUserId(int userId);
        void SetCurrentRoleId(int roleId);
        void SetCurrentCompanyId(int companyId);
        void SetCurrentOtp(OtpResponseModel otp);
        void RemoveCurrentOtp(OtpResponseModel otp);
        void SetConnectionId(string connectionId);
        void SetStatusId(int statusId);
        List<OtpResponseModel> UserOtp { get; }
    }
}
