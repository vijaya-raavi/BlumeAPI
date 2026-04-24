using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Requests.Purchase.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PurchaseController :ApiBaseController
    {

        [HttpPost("get_STS_meters_by_owner_id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OntecSelectListItem))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSTSMetersByOwnerId([FromBody]  GetSTSMetersByPropertyIdQuery request)
        {
            
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpPost("get_STS_transactions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetSTSTopUpTransactions>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSTSTopTransactions([FromBody] GetSTSPurchaseDetails request)
        {

            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }

}
