namespace Ontec.Core.Domain.Models.Dto
{
    public class UserConnectionInfo
    {
        public string UserId { get; set; }
        public string ClientType { get; set; } // e.g., "web" or "mobile"
    }
    public class LiveUserCountDto
    {
        public int Web { get; set; }
        public int Mobile { get; set; }
    }
    public class ConnectionData
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string ClientType { get; set; }
    }
}
