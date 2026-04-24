using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;

namespace Ontec.Core.Domain.Requests.Configuration.Command
{
    public  class DeleteConfigurationByIdValidator : AbstractValidator<DeleteConfigurationById>
    {
        public DeleteConfigurationByIdValidator(IConfigurationRepository _configuration,IWorkContext _workContext)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualToAsync(nameof(DeleteConfigurationById.Id),1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                int isExist = await _configuration.IsIdExist(model.Id).ConfigureAwait(false);
                if (isExist ==0)
                    context.AddFailure(nameof(DeleteConfigurationById.Id), string.Format(CommonConstants.NotExist, nameof(DeleteConfigurationById.Id)));
                if (_workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(DeleteConfigurationById.Id), CommonConstants.Unauthorized);
                }
            });
        }
    }
}
