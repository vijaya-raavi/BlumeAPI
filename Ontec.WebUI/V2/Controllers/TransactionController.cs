using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.Domain.Requests.Transaction.Queries;
namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpPost("get_transacition_masters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TransactionMasterDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransactionMastersByOwnerId([FromBody] GetTransactionMasterQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("get_transacition_Summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TransactionSummaryDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransactionsummary([FromBody] GetTransactionSummaryQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("download-statement")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadStatement([FromBody] DownloadStatementPdfQuery request)
        {

            //byte[] pdfBytes = await Mediator.Send(request).ConfigureAwait(false);
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
            //return File(pdfBytes, "application/pdf", request.MeterNumber +".pdf");
        }
    }
}
