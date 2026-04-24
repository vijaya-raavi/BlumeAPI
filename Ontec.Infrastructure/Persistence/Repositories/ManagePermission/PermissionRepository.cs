using Dapper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.ManagePermission;
using Ontec.Core.Domain.Models.Dto.ManagePermissions;
using Ontec.Core.Domain.Requests.ManagePermissions.Command;
using Ontec.Core.Domain.Requests.ManagePermissions.Queries;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Ontec.Infrastructure.Persistence.Repositories.ManagePermission
{

    public class PermissionRepository(IGenericRepository genericRepository, IWorkContext workContext, IAuditTrail auditTrail) : IPermissionRepository
    {
        private readonly IGenericRepository _genericRepository = genericRepository;
        private readonly IWorkContext _workContext=workContext;
        private readonly IAuditTrail _auditTrail = auditTrail;

        #region ManagePermissions

        public async Task AddPermissions(ManagePermissionQuery request)
        {
            try
            {
                foreach (var permission in request.Permissions)
                {
                    var sQuery = @" INSERT INTO public.ohd_manage_permission(
	                           role_id, user_id, setting_type_id, canedit, canview, candelete, created_at)
                              VALUES (@role_id
                               , @user_id
                               , @setting_type_id
                               , @canedit
                               , @canview
                               , @candelete
                               , @created_at)
                              RETURNING lastval()";
                    var parameters = new DynamicParameters();
                    parameters.Add("@role_id", permission.RoleId);
                    parameters.Add("@user_id", permission.UserId);
                    parameters.Add("@setting_type_id", permission.SettingTypeId);
                    parameters.Add("@canedit", permission.CanEdit ? 1 : 0);
                    parameters.Add("@canview", permission.CanView ? 1 : 0);
                    parameters.Add("@candelete", permission.CanDelete? 1 : 0);
                    parameters.Add("@Created_at", DateTime.UtcNow);

                    var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<int> IsUserPermissionsExsist(int userId)
        {
            var sQuery = @"SELECT Id
                                FROM ohd_manage_permission  
                              WHERE user_id=@UserId";
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsPermissionExistForOperator(int userId)
        {
            var sQuery = @"Select COUNT(user_id) 
                         FROM public.ohd_manage_permission
                         WHERE user_id=@UserId";
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;

        }

        public async Task UpdateUserPermissions(ManagePermissionQuery request)
        {
            var objAudit = new AuditHelper();
            foreach (var permission in request.Permissions)
            {
                var sQuery = @" UPDATE ohd_manage_permission
                                SET canedit=@canedit, 
                                    canview=@canview, 
                                    candelete=@candelete                                  
                                   ,modified_at = @ModifiedAt
                                 WHERE id = @Id ;
                                Select Id From ohd_manage_permission
                                 WHERE id = @Id ";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", permission.Id);
                parameters.Add("@setting_type_id", permission.SettingTypeId);
                parameters.Add("@canedit", permission.CanEdit ? 1 : 0);
                parameters.Add("@canview", permission.CanView ? 1 : 0);
                parameters.Add("@candelete", permission.CanDelete ? 1 : 0);
                parameters.Add("@ModifiedAt", DateTime.UtcNow);

                await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);

                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.AddedBy = _workContext.CurrentUserId;
                    objAudit.Action = "Update User Permission";
                    objAudit.ActionTable = "ohd_manage_permission";
                    objAudit.ModuleName = "Operator";
                    objAudit.StatusId = 0;
                    objAudit.UpdatedId = permission.Id;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
            }
            
        }
        #endregion

        public async Task<IEnumerable<SettingTypeDto>> GetSettingsTypeId(GetPermissionsQuery request)
        {
            var sQuery = @"SELECT 
                            mp.id AS Id,
                            mp.setting_type_id AS SettingTypeId,
                            st.settingtype AS SettingType,
                            st.display_name as DisplayName,
                            mp.canedit,
                            mp.canview,
                            mp.candelete 
                            FROM public.ohd_manage_permission AS mp
                            LEFT JOIN ohd_setting_type AS st ON mp.setting_type_id= st.id
                            where user_id =@UserId
                            order by st.priority desc";

            var parameter = new DynamicParameters();
            parameter.Add("@UserId", request.UserId);
            return await _genericRepository.GetAsync<SettingTypeDto>(sQuery,parameter);
        }
        public async Task<IEnumerable<SettingTypeDto>> GetRoleMaster()
        {
            var sQuery = @"SELECT 
                            id AS SettingTypeId,
                            settingtype AS SettingType
                            FROM public.ohd_setting_type";
            return await _genericRepository.GetAsync<SettingTypeDto>(sQuery);
        }
    }
}
