using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.EmailTemplate;

namespace Ontec.Core.Domain.Requests.EmailTemplates.Command
{
    public  class AddEditEmailTemplateCommandRequestValidator:AbstractValidator<AddEditEmailTemplateCommandRequest>
    {
        public AddEditEmailTemplateCommandRequestValidator(IEmailTemplateRepository emailTemplateRepository) 
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualToAsync(nameof(AddEditEmailTemplateCommandRequest.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                int count = await emailTemplateRepository.IsEmailTemplateIdExists(model.Id).ConfigureAwait(false);
                if (count==0)
                {
                    context.AddFailure(nameof(AddEditEmailTemplateCommandRequest.Id), string.Format(CommonConstants.NotExist, nameof(AddEditEmailTemplateCommandRequest.Id)));
                }
            });
        }
    }
}
