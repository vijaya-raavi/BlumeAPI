using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.ManagePermission;
using Ontec.Core.Domain.Models.Dto.ManagePermissions;
using Ontec.Core.Domain.Requests.ManagePermissions.Queries;

namespace Ontec.Core.Application.ManagePermissions.Queries
{
    public class GetPermissionMasterQueryHandler : IRequestHandler<GetPermissionsQuery, IEnumerable<SettingTypeDto>>
    {
        private readonly IPermissionRepository _permissionrRepository;

        public GetPermissionMasterQueryHandler(IPermissionRepository permissionrRepository)
        { 
            _permissionrRepository = permissionrRepository;
        }
        public async Task<IEnumerable<SettingTypeDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetPermissionsQueryValidator(_permissionrRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            return await _permissionrRepository.GetSettingsTypeId(request).ConfigureAwait(false);
        }
    }
}
