using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Models.Dto;

namespace Ontec.Infrastructure.Services
{
    public class LiveUserService : ILiveUserService
    {
        private readonly Dictionary<string, UserConnectionInfo> _connections = new();
        private readonly object _lock = new();
        public void InitializeConnections(IEnumerable<ConnectionData> connections)
        {
            lock (_lock)
            {
                _connections.Clear();
                foreach (var conn in connections)
                {
                    _connections[conn.ConnectionId] = new UserConnectionInfo
                    {
                        UserId = conn.UserId,
                        ClientType = string.IsNullOrWhiteSpace(conn.ClientType) ? "unknown" : conn.ClientType.ToLower()
                    };
                }
            }
        }

        public void AddUser(string connectionId, string userId, string clientType)
        {
            lock (_lock)
            {
                //_connections[connectionId] = userId;
                //clientType = clientType.ToLower();

                _connections[connectionId] = new UserConnectionInfo
                {
                    UserId = userId,
                    ClientType = clientType.ToLower()
                };
            }
        }

        public void RemoveUser(string connectionId)
        {
            lock (_lock)
            {
                _connections.Remove(connectionId);
            }
        }

        //public int GetLiveUserCount(string?clientType)
        //{
        //    //lock (_lock)
        //    //{
        //    //    return _connections.Values.Distinct().Count();
        //    //}
          
        //        lock (_lock)
        //        {
        //            var query = _connections.Values.AsEnumerable();

        //            if (!string.IsNullOrEmpty(clientType))
        //            {
        //                query = query.Where(x => x.ClientType.Equals(clientType, StringComparison.OrdinalIgnoreCase));
        //            }

        //            return query.Select(x => x.UserId).Distinct().Count();
        //        }
            
        //}
        public LiveUserCountDto GetLiveUserCount()
        {
            lock (_lock)
            {
                var grouped = _connections.Values
                    .GroupBy(x => x.ClientType.ToLower())
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.UserId).Distinct().Count()
                    );

                return new LiveUserCountDto
                {
                    Web = grouped.ContainsKey("web") ? grouped["web"] : 0,
                    Mobile = grouped.ContainsKey("mobile") ? grouped["mobile"] : 0
                };
            }
        }
    }
}
