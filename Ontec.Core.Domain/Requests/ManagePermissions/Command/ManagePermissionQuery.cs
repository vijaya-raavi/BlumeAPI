using MediatR;

namespace Ontec.Core.Domain.Requests.ManagePermissions.Command
{
    public class ManagePermissionQuery : IRequest<string>
    {
        public List<Permission> Permissions { get; set; }

        public class Permission
        {
            public int Id { get; set; }
            public int SettingTypeId { get; set; }
            public int RoleId { get; set; }
            public int UserId { get; set; }
            public Boolean CanEdit { get; set; }
            public Boolean CanView { get; set; }
            public Boolean CanDelete { get; set; }
        }

    }
}
