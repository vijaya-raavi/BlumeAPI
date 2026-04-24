using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.ManagePermissions
{
    public class ManagePermissionsDto
    {
        
        public int SettingTypeId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public bool IsPrmitted { get; set; }
        public Boolean CanEdit { get; set; }
        public Boolean CanView { get; set; }
        public Boolean CanDelete { get; set; }
        public string CreatedAt {  get; set; }
    }
}
