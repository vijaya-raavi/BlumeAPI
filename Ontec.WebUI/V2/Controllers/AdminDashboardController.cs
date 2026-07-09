using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.AdminDashboard;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.Domain.Requests.AdminDashboard.Queries;
using Ontec.Core.Domain.Requests.Dashboard.Queries;
using Ontec.Core.Domain.Requests.Transaction.Queries;

namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AdminDashboardController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpGet("get_Payment_dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentDashboardDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentDashboard()
        {
            var request = new GetPaymentDashboardQuery
            {
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [MapToApiVersion("2.0")]
        [HttpPost("get_admin__dashboard_masters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminDashboardMastersDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAdminDashboardMaster([FromBody] GetAdminDashboardMasterQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get_admin_dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminDashboardDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var request = new GetAdminDashboardRequestQuery
            {
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
