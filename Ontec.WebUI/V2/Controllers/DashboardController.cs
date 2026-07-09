using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Consumer.Queries;
using Ontec.Core.Domain.Requests.Dashboard;
using Ontec.Core.Domain.Requests.Dashboard.Command;
using Ontec.Core.Domain.Requests.Dashboard.Queries;
using Ontec.Core.Domain.Requests.Transaction.Queries;

namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpGet("get_account_balance/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyAccountBalanceDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAccountBalanceByPropertyId(int propertyId)
        {
            var request = new GetAccountBalanceByPropertyIdQuery
            {
                PropertyId = propertyId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get_property_details_by_ownerId/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDetailsDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPropertyDetailsDashboard([FromRoute] int id)
        {
            var request = new GetPropertyDetailsQuery
            {
                UserId = id,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get_dashboard_masters_by_ownerId/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DashboardMastersDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardMasters([FromRoute] int userId)
        {
            var request = new GetDashboardMastersQuery
            {
                UserId = userId,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get_meter_consumption/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DashboardMeterDayConsumptionDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPropertMetersConsuption([FromRoute] int propertyId)
        {
            var request = new GetDashboardMeterDayConsumptionQuery
            {
                PropertyId = propertyId,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get_meter_and_user_request_count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeterAndUserRequestDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMeterAndUserRequestCount()
        {
            var request = new GetMeterAndUserRequestCount
            {
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("capture_credits")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CaptureCredits(CaptureUsersCreditsToSaveCommandReuqest request)
        {

            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get_credits/{meterId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyAccountBalanceDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCreditsByMeterId(int meterId)
        {
            var request = new GetCreditsByMeterIdQuery
            {
                MeterId = meterId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
