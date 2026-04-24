namespace Ontec.Core.Domain.Models.Dto.UserSettings
{
    public class UserSettingDto:BaseModel
    {
        public int Id {  get; set; }
        public int UserId { get; set; }
        public bool IsEmailEnabled { get; set; }
        public bool IsMobileEnabled { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedAt { get; set; }
    }
}
