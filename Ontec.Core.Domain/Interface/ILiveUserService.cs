using Ontec.Core.Domain.Models.Dto;

namespace Ontec.Core.Domain.Interface
{
    public interface ILiveUserService
    {
        void AddUser(string connectionId, string userId,string clientType);
        public  void RemoveUser(string connectionId);
        public LiveUserCountDto GetLiveUserCount();
        void InitializeConnections(IEnumerable<ConnectionData> connections);
    }


}
