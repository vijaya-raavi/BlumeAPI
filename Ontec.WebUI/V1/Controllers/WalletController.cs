using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.Wallet.Command;
using Ontec.Core.Domain.Requests.Wallet.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : ApiBaseController
    {
        [HttpPost("process_payment")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetUserCardsDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SaveWalletTransactions([FromForm] UpdateWalletBalanceQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpPost("get_wallet_by_user_id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserWalletDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWallet([FromBody] GetUserWalletQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpPost("get_wallet_transactions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserWalletTransactionDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWalletTransactions([FromBody] GetUserWalletTransactionQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

    }
}
