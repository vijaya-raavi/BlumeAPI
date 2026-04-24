using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;

namespace Ontec.Core.Domain.Requests.Configuration.Queries
{
    public  class GetConfigurationQueryValidator: AbstractValidator<GetConfigurationQuery>
    {
        public GetConfigurationQueryValidator(IWorkContext _workContext)
        {
           
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                if(_workContext.CurrentRoleId!=(int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(_workContext.CurrentRoleId), CommonConstants.Unauthorized);
                }
            });
        }
    }
}
