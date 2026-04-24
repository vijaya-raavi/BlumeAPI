using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Dashboard.Queries;

namespace Ontec.Core.Domain.Requests.AdminDashboard.Queries
{
    public class GetAdminDashboardMasterQueryValidator: AbstractValidator<GetAdminDashboardMasterQuery>
    {
        public GetAdminDashboardMasterQueryValidator(IUserRepository _userRepository,IWorkContext workContext)
        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(GetAdminDashboardMasterQuery.UserId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetAdminDashboardMasterQuery.UserId),"User id does not exist");
                

                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                    context.AddFailure(nameof(GetAdminDashboardMasterQuery.UserId), string.Format(CommonConstants.Unauthorized, nameof(GetAdminDashboardMasterQuery.UserId)));
            });
        }
    }
}
