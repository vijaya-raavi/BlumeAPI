
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Models.Dto.Debitech;
using Ontec.Core.Domain.Requests.Debitech.Command;
using Ontec.Core.Domain.Requests.Debitech.Queries;
using System.ComponentModel.DataAnnotations;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class DebiTechController : ApiBaseController
    {
        private readonly MasterApiSetting _masterApiSetting;
        public DebiTechController(MasterApiSetting masterApiSetting)
        {
            _masterApiSetting = masterApiSetting;
        }
        [HttpPost("notify_bank_transfer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BankNotificationResponseDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateBankAccount([FromBody] IEnumerable<BankNotificationRequest> request, [FromHeader(Name = "api-key")] string apiKey)
        {
         
            if (string.IsNullOrEmpty(apiKey) || apiKey.ToString() != _masterApiSetting.DebitechApiKey)
            {
                return BadRequest("Invalid api key.");
            }

            if (request != null && !request.Any(t=>t.NetUpChecksum.Equals("string")))
            {
                var multiBankRequest = new BankNotificationRequestQuery
                {
                    BankNotificationRequest = request
                };
                return Ok(await Mediator.Send(multiBankRequest).ConfigureAwait(false));
            }else
            {
                return BadRequest();
            }
        }

        [HttpPost("get_net_up_checksum")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNetUpCheckSum([FromBody] GetNetCheckSumQuery request)
        {

            if (request != null)
            {
                return Ok(await Mediator.Send(request).ConfigureAwait(false));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
