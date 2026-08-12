using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Configuration;
using Ontec.Core.Domain.Requests.Configuration.Command;
using Ontec.Core.Domain.Requests.Configuration.Queries;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Command;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Queries;

namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ConfigurationController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpPost("add-update-configuration")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateConfiguration([FromBody] AddOrUpdateConfigurationQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("get_configurations")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConfigurationDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConfigurations([FromBody] GetConfigurationQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [MapToApiVersion("2.0")]
        [HttpDelete("delete_configurations-by-id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteConfigurationById([FromBody] int id)
        {
            var request = new DeleteConfigurationById
            {
                Id = id
            };

            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("update-app_backgound_pic")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAppGround([FromForm] UpdateAppBackgroundImage request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("add-update-rejection_reason")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateRejectionReason([FromBody] AddUpdateRejectionReasonQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("get_rejection_reason")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RejectionReasonDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRejectionReason([FromBody] GetRejectionReasonQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("update-status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("update_business_configuration")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatebusinessConfugurations([FromBody] UpdateBusinessHoursConfigurations request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
