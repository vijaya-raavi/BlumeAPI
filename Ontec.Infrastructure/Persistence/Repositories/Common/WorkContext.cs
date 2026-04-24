using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Models.Dto.Otp;

namespace Ontec.Infrastructure.Persistence.Repositories.Common
{
    public class WorkContext : IWorkContext
    {
        private int _currentUserId;
        private int _currentRoleId;
        private string _connectionId;
        private int _statusId;
        private int _currentCompanyId;

        private List<OtpResponseModel> _userOtp;
        public int CurrentUserId => _currentUserId;
        public int CurrentCompanyId => _currentCompanyId;
        public int CurrentRoleId => _currentRoleId;

        public string ConnectionId => _connectionId;

       public int StatusId => _statusId;
        public List<OtpResponseModel> UserOtp => _userOtp ?? new List<OtpResponseModel>();

        int IWorkContext.StatusId => throw new NotImplementedException();

        public void SetCurrentOtp(OtpResponseModel otp)
        {
            if (_userOtp == null)
            {
                _userOtp = new List<OtpResponseModel>();
            }
            _userOtp.Add(otp);
        }
        public void RemoveCurrentOtp(OtpResponseModel otp)
        {
            var type = _userOtp.Single(r => r.Type == otp.Type);
            _userOtp.Remove(type);
        }

        public void SetCurrentRoleId(int roleId)
        {
            _currentRoleId = roleId;
        }
        public void SetCurrentUserId(int userId)
        {
            _currentUserId = userId;
        }
        public void SetCurrentCompanyId(int companyId)
        {
            _currentCompanyId = companyId;
        }
        public void SetConnectionId(string connectionId)
        {
            _connectionId = connectionId;
        }

        public void SetStatusId(int statusId)
        {
            _statusId = statusId;
        }
    }
}
