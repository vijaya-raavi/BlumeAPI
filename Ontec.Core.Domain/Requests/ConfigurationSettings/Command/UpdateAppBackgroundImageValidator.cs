using FluentValidation;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class UpdateAppBackgroundImageValidator : AbstractValidator<UpdateAppBackgroundImage>
    {
        public UpdateAppBackgroundImageValidator(IConfigurationRepository _configurationRepository, IWorkContext _workcontext)
        {
            RuleFor(x => x.AppBackgroundImage).IsImageValid(nameof(UpdateAppBackgroundImage.AppBackgroundImage), 5); // 5 MB 
            RuleFor(x => x.Id).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(UpdateAppBackgroundImage.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (_workcontext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(_workcontext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(_workcontext.CurrentRoleId)));
                }

                if (model.AppBackgroundImage == null)
                {
                    context.AddFailure(nameof(UpdateAppBackgroundImage.AppBackgroundImage), string.Format(CommonConstants.IsRequired, nameof(UpdateAppBackgroundImage.AppBackgroundImage)));
                }
                int id = await _configurationRepository.IsConfigIdExist(model.Id).ConfigureAwait(false);
               
                if (id==0)
                {
                    context.AddFailure(nameof(UpdateAppBackgroundImage.Id), string.Format(CommonConstants.NotExist, nameof(UpdateAppBackgroundImage.Id)));
                }
            });
        }
    }
}
