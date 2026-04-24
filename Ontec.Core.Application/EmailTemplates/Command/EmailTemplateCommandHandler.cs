using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.EmailTemplates.Command;

namespace Ontec.Core.Application.EmailTemplates.Command
{
    public class EmailTemplateCommandHandler : IRequestHandler<AddEditEmailTemplateCommandRequest, AddUpdateResultDto>
    {
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        public EmailTemplateCommandHandler(IEmailTemplateRepository emailTemplateRepository)
        {
            _emailTemplateRepository = emailTemplateRepository;
        }
        public async Task<AddUpdateResultDto> Handle(AddEditEmailTemplateCommandRequest request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new AddEditEmailTemplateCommandRequestValidator(_emailTemplateRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;

            result = await _emailTemplateRepository.UpdateEmailTemplate(request).ConfigureAwait(false);
            if (result > 0)
                response.Id = result;
            response.Message = "Email template updated successfully";

            return response;
        }
    }
}
