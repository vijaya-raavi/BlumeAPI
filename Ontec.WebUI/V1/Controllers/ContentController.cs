using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Content;
using Ontec.Core.Domain.Requests.Content.Command;
using Ontec.Core.Domain.Requests.Content.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : ApiBaseController
    {
        [HttpPost("get_contents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContentMaster))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConent([FromBody] GetContentQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpPost("update_contents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateConent([FromBody] AddContentQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
