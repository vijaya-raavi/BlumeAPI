using MediatR;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Requests.EmailTemplates.Queries;

namespace Ontec.Core.Application.EmailTemplates.Queries
{
    public class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplateMasterQuery, IEnumerable<EmailTemplateDto>>
    {
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        public GetEmailTemplatesQueryHandler(IEmailTemplateRepository emailTemplateRepository)
        {
            _emailTemplateRepository = emailTemplateRepository;
        }
        public async Task<IEnumerable<EmailTemplateDto>> Handle(GetEmailTemplateMasterQuery request, CancellationToken cancellationToken)
        {

            return await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
        }
    }
}
