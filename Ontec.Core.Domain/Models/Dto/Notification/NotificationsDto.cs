namespace Ontec.Core.Domain.Models.Dto.Notification
{
    public class NotificationsDto
    {

        public IEnumerable<UserNotification>? Notification { get; set; }
        public int Count {  get; set; }

    }
    public class UserNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NotificationType { get; set; }
        public bool IsRead { get; set; }
        public string NoteStatus {  get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModifiedAt { get; set; }
        public string TimeAgo { get; set; }
    }
}
