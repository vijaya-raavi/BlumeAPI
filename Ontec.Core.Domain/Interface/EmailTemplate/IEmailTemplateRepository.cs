using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Requests.EmailTemplates.Command;

namespace Ontec.Core.Domain.Interface.EmailTemplate
{
    public interface IEmailTemplateRepository
    {
        Task<IEnumerable<EmailTemplateDto>> GetEmailTemplates();
        Task<int> IsEmailTemplateIdExists(int id);
        Task<int> UpdateEmailTemplate(AddEditEmailTemplateCommandRequest request);
    }
}
