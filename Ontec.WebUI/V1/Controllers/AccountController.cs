using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Account;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Requests.Account.Queries;
using Ontec.Core.Domain.Requests.Dashboard.Queries;
namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ApiBaseController
    {

        [HttpGet("get_Account_masters_by_ownerId/{userId}")]
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

        [HttpGet("get_account_balance/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountBalanceDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAccountBalanceByPropertyId(int propertyId)
        {
            var request = new GetAccountBalanceByPropertyId
            {
                PropertyId = propertyId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpGet("get_account_history/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountBalanceDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAccountHistory(int propertyId)
        {
            var request = new GetAccountHisotryQuery
            {
                PropertyId = propertyId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
    }
}
