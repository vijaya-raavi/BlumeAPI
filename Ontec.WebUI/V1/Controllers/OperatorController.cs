using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Operator.Command;
using Ontec.Core.Domain.Requests.Operator.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class OperatorController : ApiBaseController
    {
        [HttpPost("get_operators")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DatatableModel<OperatorDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOperators([FromBody] GetOperatorsQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }


        [HttpPost("Add-Update-Operator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateOperatorUser([FromBody] AddOrUpdateOperatorQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("delete-operator-user/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteOperatorUserById([FromRoute] int id)
        {
                var request = new DeleteOperatorUserById
                {
                    Id = id
                };

                return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }

    
}
