using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.UserRole;

namespace Ontec.Core.Domain.Interface.User
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRoleDto>> GetUserRoles();
        Task<IEnumerable<OntecSelectListItem>> GetRoleMasters();
        Task<IEnumerable<OntecSelectListItem>> GetTitleMasters();
    }
}
