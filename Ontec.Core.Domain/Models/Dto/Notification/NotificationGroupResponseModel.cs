namespace Ontec.Core.Domain.Models.Dto.Notification
{
    public class NotificationGroupResponseModel
    {
        public string NotificationKey {  get; set; }
        public int Id {  get; set; }
        public string Message {  get; set; }
    }
    public class TopicManagementResponseDto
    {
        public int SuccessCount { get; }
        public int FailureCount { get; }
        public IReadOnlyList<TopicManagementError> Errors { get; }
    }
    public sealed class TopicManagementError
    {
        public int Index { get; } // index in your input token list
        public string Reason { get; } // reason why it failed
    }
}
