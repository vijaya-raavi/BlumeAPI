using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Requests.EmailTemplates.Command;
using Ontec.Core.Domain.Requests.EmailTemplates.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    public class EmailTemplateController : ApiBaseController
    {
        [HttpGet("get_email_templates_masters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmailTemplateDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEmailTemplateMasters()
        {
            var request = new GetEmailTemplateMasterQuery()
            { };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("add_edit_email_templates")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult>EditEmailTemplates(AddEditEmailTemplateCommandRequest request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
