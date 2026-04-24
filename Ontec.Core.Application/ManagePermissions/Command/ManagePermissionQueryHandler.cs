using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.ManagePermission;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.ManagePermissions.Command;

namespace Ontec.Core.Application.ManagePermissions.Command
{
    public class ManagePermissionQueryHandler : IRequestHandler<ManagePermissionQuery, string>
    {
        private readonly IPermissionRepository _permissionrRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWorkContext _workContext;
        public ManagePermissionQueryHandler(IPermissionRepository permissionRepository, IUserRepository userRepository, IWorkContext workContext)
        {
            _permissionrRepository = permissionRepository;
            _workContext = workContext;
            _userRepository = userRepository;
        }
        public async Task<string> Handle(ManagePermissionQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();


            var commonValidator = new ManagePermissionQueryValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            string message = "";

            await _permissionrRepository.UpdateUserPermissions(request).ConfigureAwait(false);
            message = "Records permissions updated successfully!";

            return message;
        }
    }
}
