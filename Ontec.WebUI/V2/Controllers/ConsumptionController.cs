using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Charts;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Requests.Consumption.Queries;

namespace Ontec.WebUI.V2.Controllers
{

    [Route("api/[controller]")]
    [Authorize]
    public class ConsumptionController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpPost("get_consumption_masters_by_ownerId")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsumptionMastersDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumptionMastersByOwnerId([FromBody] GetConsumptionMastersQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("get_consumption_dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsumptionDashboardDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumptionDashboard([FromBody] GetConsumptionDashboardQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("get_consumption_houlry_dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LineChartDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumptionHourlyDashboard([FromBody] GetConsumptionHourlyDashboardQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
