using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.PropertyUser;
using Ontec.Core.Domain.Requests.PropertyUser.Command;
using Ontec.Core.Domain.Requests.PropertyUser.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PropertyUserController : ApiBaseController
    {
        [HttpGet("get-property-user-masters/{ownerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EditPropertyUserMasters>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPropertyUserMastersByOwnerId([FromRoute] int ownerId)
        {
            var request = new GetPropertyUserMastersByOwnerIdQuery
            {
                OwnerId = ownerId,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddEditPropertyUser>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPropertyUserById([FromRoute] int id)
        {
            var request = new GetPropertyUserById
            {
                Id = id
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("delete-property-user/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePropertyUserById([FromRoute] int id)
        {
            var request = new DeletePropertyUserById
            {
                Id = id,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("add-update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdatePropertyUser([FromBody] AddUpdatePropertyUser request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("get_property_userlist/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyUserDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPropertyUserListByPropetyId([FromRoute] int propertyId)
        {
            var request = new GetPropertyUserListByPropertyIdQuery
            {
                PropertyId = propertyId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpDelete("delete-associates-by-propertyid/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePropertyAssociateUserById([FromRoute] int propertyId)
        {
            var request = new DeletePropertyAssociateUserByPropertyId
            {
                PropertyId= propertyId,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("associtae-user_settings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateAssociateUserSettings([FromBody] AddUpdateAssociateUserSettingsQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

    }
}