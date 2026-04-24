using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.VendRequest;
using Ontec.Core.Domain.Requests.TopUp.Command;
using Ontec.Core.Domain.Requests.TopUp.Queries;
using Ontec.Core.Domain.Requests.VendRequest.Commands;
using static Ontec.Core.Domain.Models.Dto.STSPurchase.STSPurchasesDto;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TopUpController : ApiBaseController
    {
        private readonly ILogger<TopUpController> _logger;
        private readonly IDocumentRepository _documentRepository;
        public TopUpController(ILogger<TopUpController> logger, IDocumentRepository documentRepository)
        {
            _logger = logger;
            _documentRepository = documentRepository;
        }
        [HttpPost("add_update_bank_account")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateBankAccount([FromBody] AddOrUpdateBankAccountQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpPost("get_bank_accounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BankAccountDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BankAcccounts([FromBody] GetBankAccountsQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpDelete("delete_bank_account/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteBankAccountById([FromRoute] int id)
        {
            var request = new DeleteBankAccountbyId
            {
                Id = id
            };

            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpPost("add_topup_transaction")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddTopUpTransactionsDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddTopUpTransaction([FromBody] AddTopUpTransactionQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("get_payment_methods")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PaymentMethodsDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PaymentMethods([FromBody] GetPaymentMethodsQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [HttpPost("update_payment_methods")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePaymentMethods([FromBody] UpdatePaymentMethodsQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [AllowAnonymous]
        [HttpPost("update_notified_topup_transaction")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VendRequestResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateNotifyTopUpTransaction([FromForm] PayFastModel request)
        {
            try
            {
                _logger.LogInformation("Entered UpdateNotifyTopUpTransaction");
                return Ok(await Mediator.Send(request).ConfigureAwait(false));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateNotifyTopUpTransaction");
               
                var applicationLogger = new ApplicationLogger
                {
                    Request = JsonConvert.SerializeObject(request),
                    Method = "DownloadStatementPdfQuery_Handle",
                    Error = ex.Message
                };
                _ = await _documentRepository.AddApplicationLogger(applicationLogger).ConfigureAwait(false);
            
            throw;
            }
        }
        [AllowAnonymous]
        [HttpPost("update_wallet_topup_transaction")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VendRequestResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateWalletTopUpTransaction([FromForm] WalletTopUpModel request)
        {
            try
            {
                _logger.LogInformation("Entered UpdateWalletTopUpTransaction");


                return Ok(await Mediator.Send(request).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateWalletTopUpTransaction");
                throw;
            }
        }

        [HttpPost("get_top_up_transactions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddTopUpTransactionsDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTopUpTransactions([FromBody] GetTopUpTransactionsQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }


        [HttpPost("Cancel_top_up_transaction")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelTopUpTransaction([FromBody] CancelTopUpTransactionQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("get_user_payments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserPaymentsDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPayament([FromBody] GetUserPayamenstQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [AllowAnonymous]
        [HttpPost("Text_Vend_Request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VendRequestResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendTestVendRequest([FromBody] SendVendRequestCommand request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }



        [HttpPost("download-purchace-receipt")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadPurchaceReceipt([FromBody] DownloadPurchaceRecieptPdfQuery request)
        {
            //byte[] pdfBytes = await Mediator.Send(request).ConfigureAwait(false);
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
            //return File(pdfBytes, "application/pdf", request.MeterNumber +".pdf");
        }



    }

}
