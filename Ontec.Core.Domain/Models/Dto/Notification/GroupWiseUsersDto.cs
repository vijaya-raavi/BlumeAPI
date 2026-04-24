namespace Ontec.Core.Domain.Models.Dto.Notification
{
    public  class GroupWiseUsersDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName {  get; set; }
        public int UserId { get; set; }
        public string Customer {  get; set; }
        public string DeviceToken {  get; set; }
        public string StatusId {  get; set; }

        public string NotificationKey {  get; set; }

    }

    public class GroupWiseDetailedDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string NotificationKey { get; set; }
        public IEnumerable<UserDetails> UserList { get; set; }
    }
    public class UserDetails
    {
        public int UserId { get; set; }
        public string Customer { get; set; }
        public string DeviceToken {  get; set; }

    }
}
