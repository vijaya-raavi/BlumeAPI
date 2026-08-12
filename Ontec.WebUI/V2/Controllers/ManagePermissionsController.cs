using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.ManagePermissions;
using Ontec.Core.Domain.Requests.Dashboard.Queries;
using Ontec.Core.Domain.Requests.ManagePermissions.Command;
using Ontec.Core.Domain.Requests.ManagePermissions.Queries;

namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ManagePermissionsController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpPost("manage_permissions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ManagePermissionsDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ManagePermissions([FromBody] ManagePermissionQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("get_operator_permissions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SettingTypeDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPermissions([FromBody] GetPermissionsQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
