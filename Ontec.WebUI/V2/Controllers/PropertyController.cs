using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Property.Command;
using Ontec.Core.Domain.Requests.Property.Handler;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.WebUI.V2.Controllers
{

    [Route("api/[controller]")]
    [Authorize]
    public class PropertyController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpPost("get_property_by_ownerId")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPropertyByOwnerId([FromBody] GetPropertiesByOwnerIdQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("edit")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateProperty([FromBody] AddOrUpdatePropertyQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PropertyModelDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPropertyById([FromRoute] int id, int companyId)
        {
            var request = new GetPropertyByIdQuery
            {
                Id = id,
                CompanyId = companyId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("delete-property")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePropertyById([FromBody] DeletePropertyById request)
        {
            
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #region ForGetAllProperties
        [MapToApiVersion("2.0")]
        [HttpPost("get-all-properties")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DatatableModel<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllProperties([FromBody] GetAllPropertiesRequestQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #endregion


        [MapToApiVersion("2.0")]
        [HttpPost("active-property/{id}/{checkValidation}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyActiveDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ActivePropertyById([FromRoute] int id, bool checkValidation)
        {
            var request = new ActivePropertyCommandRequest()
            {
                Id = id,
                CheckValidation = checkValidation
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}