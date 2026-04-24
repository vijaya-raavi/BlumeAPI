using MediatR;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;

namespace Ontec.Core.Domain.Requests.EmailTemplates.Queries
{
    public class GetEmailTemplateMasterQuery: IRequest<IEnumerable<EmailTemplateDto>>
    {
    }
}
