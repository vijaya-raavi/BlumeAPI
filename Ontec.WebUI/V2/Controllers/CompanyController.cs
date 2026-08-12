using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Company;
using Ontec.Core.Domain.Requests.Company.Command;
using Ontec.Core.Domain.Requests.Company.Queries;
using Ontec.Core.Domain.Requests.Consumer.Queries;

namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]

    public class CompanyController : ApiBaseController
    {
        [Authorize]
        [MapToApiVersion("2.0")]
        [HttpPost("Company-update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateComapny([FromBody] AddUpdateCompanyQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }


        [AllowAnonymous]
        [MapToApiVersion("2.0")]
        [HttpGet("get_company_details/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCompany([FromRoute] int companyId)
        {
            GetCompanyDetailsQuery request = new()
            {
                Id = companyId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("Company_masters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyMasters))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetcompanyMasters()
        {
            var request = new GetCompanyMastersQuery
            {
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [Authorize]
        [MapToApiVersion("2.0")]
        [HttpPost("update-CompanyLogo_pic")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCompany([FromForm] UpdateCompanyLogo request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

       
    }
}
