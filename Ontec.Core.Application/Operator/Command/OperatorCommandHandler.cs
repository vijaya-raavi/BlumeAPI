using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.ManagePermission;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.ManagePermissions.Command;
using Ontec.Core.Domain.Requests.ManagePermissions.Queries;
using Ontec.Core.Domain.Requests.Operator.Command;

namespace Ontec.Core.Application.Operator.Command
{
    public class AddOrUpdateOperatorCommandHandler : IRequestHandler<AddOrUpdateOperatorQuery, AddUpdateResultDto>,
                                                     IRequestHandler<DeleteOperatorUserById, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkContext _workContext;
        private readonly IEncryptionandDecryption _encryptionandDecryption;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuditTrail _auditTrail;

        public AddOrUpdateOperatorCommandHandler(IWorkContext workContext, IUserRepository userRepository
            , IEncryptionandDecryption encryptionandDecryption, IPermissionRepository permissionRepository, IAuditTrail auditTrail)
        {
            _workContext = workContext;
            _userRepository = userRepository;
            _encryptionandDecryption = encryptionandDecryption;
            _permissionRepository = permissionRepository;
            _auditTrail = auditTrail;
        }
        public async Task<AddUpdateResultDto> Handle(AddOrUpdateOperatorQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new AddOrUpdateOperatorQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;
            request.RoleId = (int)RoleMasterEnum.Operator;

            request.Password = _encryptionandDecryption.Encrypt(request.Password);
            if (request.Id > 0)
            {
                result = await _userRepository.UpdateOperatorUser(request).ConfigureAwait(false);
                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.ModifiedBy = _workContext.CurrentUserId;
                    objAudit.Action = "Update Operator";
                    objAudit.ActionTable = "ohd_user";
                    objAudit.ModuleName = "Operator";
                    objAudit.StatusId = (int)StatusEnum.Active;
                    objAudit.UpdatedId = result;
                    objAudit.EntityName = request.FirstName + " " + request.LastName;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
            }
            else
            {
                result = await _userRepository.AddOperatorUser(request).ConfigureAwait(false);
                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
                {
                    objAudit.AddedBy = _workContext.CurrentUserId;
                    objAudit.Action = "Add Operator";
                    objAudit.ActionTable = "ohd_user";
                    objAudit.ModuleName = "Operator";
                    objAudit.StatusId = (int)StatusEnum.Active;
                    objAudit.UpdatedId = result;
                    objAudit.EntityName = request.FirstName + " " + request.LastName;
                    await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
                }
                if (result > 0)
                {
                    try
                    {

                        GetPermissionsQuery getPermissionMasterQuery = new GetPermissionsQuery
                        {
                            UserId = result

                        };

                        var settingsIds = await _permissionRepository.GetRoleMaster();
                        var settingIdList = settingsIds.ToList();

                        var managePermission = new ManagePermissionQuery
                        {
                            Permissions = [] // Ensure this list exists in your query object.
                        };

                        foreach (var settingId in settingIdList) // Iterate over each setting type.
                        {
                            var permission = new ManagePermissionQuery.Permission
                            {
                                RoleId = (int)RoleMasterEnum.Operator,
                                UserId = result,
                                SettingTypeId = settingId.SettingTypeId, // Assuming settingId has Id.
                                CanEdit = false,
                                CanDelete = false,
                                CanView = true
                            };
                            managePermission.Permissions.Add(permission);
                        }

                        int id = await _permissionRepository.IsPermissionExistForOperator(result);
                        if (id > 0)
                        {
                            await _permissionRepository.UpdateUserPermissions(managePermission);
                        }
                        else
                        {
                            await _permissionRepository.AddPermissions(managePermission);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding permission for SettingTypeId:{ex.Message}");
                    }
                }
            }
            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Records created successfully!";
                else
                    response.Message = "Records updated successfully!";
            }
            return response;
        }

        public async Task<string> Handle(DeleteOperatorUserById request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new DeleteOperatorUserByIdValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var operatorUSer= await _userRepository.GetUserById(request.Id).ConfigureAwait(false);
            await _userRepository.DeleteOperatorUserById(request.Id).ConfigureAwait(false);
            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                objAudit.ModifiedBy = _workContext.CurrentUserId;
                objAudit.Action = "Delete Operator";
                objAudit.ActionTable = "ohd_user";
                objAudit.ModuleName = "Operator";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.UpdatedId = request.Id;
                objAudit.EntityName= operatorUSer.FirstName +" "+ operatorUSer.LastName;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            return "Deleted successfully!";
        }
    }
}
