using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.ManagePermission;

namespace Ontec.Core.Domain.Requests.ManagePermissions.Queries
{
    public class GetPermissionsQueryValidator : AbstractValidator<GetPermissionsQuery>
    {
        public GetPermissionsQueryValidator(IPermissionRepository permissionRepository)
        {
            RuleFor(x => x.UserId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetPermissionsQuery.UserId), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {

                int count = await permissionRepository.IsPermissionExistForOperator(model.UserId).ConfigureAwait(false);
                if (count==0)
                {
                    context.AddFailure(nameof(GetPermissionsQuery.UserId), string.Format(CommonConstants.PermissionNotExist, nameof(GetPermissionsQuery.UserId)));
                }

            });
        }
    
    }
}
