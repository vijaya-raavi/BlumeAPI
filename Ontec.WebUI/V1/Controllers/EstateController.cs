using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Estate;
using Ontec.Core.Domain.Models.Dto.Notification;
using Ontec.Core.Domain.Requests.Estate.Command;
using Ontec.Core.Domain.Requests.Estate.Queries;
using Ontec.Core.Domain.Requests.Notification.Command;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class EstateController : ApiBaseController
    {
        [HttpPost("edit")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateEstate([FromBody] AddEstateRequestCommand request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpDelete("delete-estate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteEstateById([FromRoute] int id)
        {
            var request = new DeleteEstateRequestCommand
            {
                Id = id,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("get_estates")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EstateDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEstates()
        {
            var request = new GetEstatesRequestQuery()
            { };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("send_notifcation_to_estate_topic")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationGroupResponseModel))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendNotificationtoEstates([FromBody] SendEstateNotificationRequest request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }

}
