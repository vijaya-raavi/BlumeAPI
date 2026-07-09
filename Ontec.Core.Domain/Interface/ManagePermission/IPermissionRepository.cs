using Ontec.Core.Domain.Models.Dto.ManagePermissions;
using Ontec.Core.Domain.Requests.ManagePermissions.Command;
using Ontec.Core.Domain.Requests.ManagePermissions.Queries;

namespace Ontec.Core.Domain.Interface.ManagePermission
{
    public interface IPermissionRepository
    {
        Task AddPermissions(ManagePermissionQuery request);
        Task<int> IsUserPermissionsExsist(int userId);
        Task UpdateUserPermissions(ManagePermissionQuery request);
        Task<IEnumerable<SettingTypeDto>> GetSettingsTypeId(GetPermissionsQuery request);
        Task<IEnumerable<SettingTypeDto>> GetSettingTypeaster();
        Task<int> IsPermissionExistForOperator(int userId);
    }
}
