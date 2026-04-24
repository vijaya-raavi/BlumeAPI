using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.Common.Queries;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ApiBaseController
    {
        [HttpPost("get_audit_trail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DatatableModel<AuditTrailDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardMasters([FromBody] GetAuditTrailQuery request)
        {
            
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

    }
}
