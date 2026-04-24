using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class AddUpdateRejectionReasonQueryValidator : AbstractValidator<AddUpdateRejectionReasonQuery>
    {
        public AddUpdateRejectionReasonQueryValidator(IConfigurationRepository configurationRepository, IWorkContext _workContext)
        {
            RuleFor(m => m.Id).NotNull();
            RuleFor(m => m.RejectionReason).NotNullAndEmptyAsync().IsValidInput().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateRejectionReasonQuery.RejectionReason).SplitPascalCase(), 50);
            RuleFor(m => m).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isExist = await configurationRepository.IsReasonExist(model).ConfigureAwait(false);

                if (isExist > 0)
                {
                    context.AddFailure(nameof(AddUpdateRejectionReasonQuery.RejectionReason), string.Format(CommonConstants.AlreadyExist, nameof(AddUpdateRejectionReasonQuery.RejectionReason)));
                }
                if (_workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(_workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(_workContext.CurrentRoleId)));
                }
            });
        }
    }
}