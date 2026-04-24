namespace Ontec.Core.Domain.Models.Dto.ManagePermissions
{
    public class SettingTypeDto
    {
        public int Id { get; set; }
        public string SettingType { get; set; }
        public string DisplayName { get; set; }
        public int SettingTypeId { get; set; }
        public bool canedit { get; set; }
        public bool canview { get; set; }
        public bool candelete { get; set; }
    }
}
