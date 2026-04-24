using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.EmailTemplates.Command
{
    public class AddEditEmailTemplateCommandRequest : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string HtmlContent {  get; set; }
    }
}
