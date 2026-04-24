using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Queries
{
    public class GetRejectionReasonQueryValidator:AbstractValidator<GetRejectionReasonQuery>
    {
        public GetRejectionReasonQueryValidator(IConfigurationRepository configurationRepository ,IWorkContext _workContext) 
        {
            RuleFor(m => m).CustomAsync(async (model, context, CancellationToken) =>
            {
                if (model.Id > 0)
                {
                    var isExist = await configurationRepository.IsReasonIdExist(model.Id).ConfigureAwait(false);

                    if (isExist == 0)
                    {
                        context.AddFailure(nameof(GetRejectionReasonQuery.Id), string.Format(CommonConstants.NotExist, nameof(GetRejectionReasonQuery.Id)));
                    }
                }
                if (_workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && _workContext.CurrentRoleId !=(int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(_workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(_workContext.CurrentRoleId)));
                }
            });
        }
    }
}
