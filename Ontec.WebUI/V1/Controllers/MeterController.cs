using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Requests.Meter.Command;
using Ontec.Core.Domain.Requests.Meter.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class MeterController : ApiBaseController
    {
        [HttpGet("get-meterby-propertyId/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PropertyMetersDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMetersByPropertyId([FromRoute] int propertyId)
        {
            var request = new GetMetersByPropertyIdQuery
            {
                Id = propertyId,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("get-meter-expiring-list/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MeterExpiringDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMetersExpiringByUserId([FromRoute] int userId)
        {
            var request = new GetMeterExpiringListRequest
            {
                UserId = userId,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UpdateMeterDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMeterById([FromRoute] int id)
        {
            var request = new GetMeterByIdQuery
            {
                Id = id,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("delete-meter/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePropertyById([FromRoute] int id)
        {
            var request = new DeleteMeterById
            {
                Id = id,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("get-meter-masters/{ownerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EditMeterMasters>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMetersMastersByOwnerId([FromRoute] int ownerId)
        {
            var request = new GetMeterMastersByOwnerIdQuery
            {
                OwnerId = ownerId,
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }


        [HttpPost("edit")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddUpdateMeter([FromForm] AddUpdateMeterQuery request)
        {
            if (request.Id == 0)
            {
                request.ContractProofDocumentTypeId = (int)DocumentTypeEnum.ContractProof;
            }
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpGet("verify-meter{PropertyId}/{MeterNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyMeter(int PropertyId, string MeterNumber)
        {
            var request = new VerifyMeterQuery()
            {
                PropertyId = PropertyId,
                MeterNumber = MeterNumber
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }



        #region ForGetMeterRequest
        [HttpPost("get-meter-request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DatatableModel<MeterRequestDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMeters([FromBody] GetMeterRequestQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #endregion

        #region ForApproveRejectMeterRequest
        [HttpPost("meter-approve-Reject-by-id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveRejectMeter([FromBody] GetApproveRejectMeterQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #endregion


        #region GetMetersFromMaster
        [HttpGet("get_meters_from_master{PropertyId}/{MeterNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MetersUtilityDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMetersbyMeterNumber(int PropertyId, string MeterNumber)
        {
            var request = new GetMetersByMeterNumberQuery()
            {
                MeterNumber = MeterNumber,
                PropertyId = PropertyId,
            };


            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #endregion
        #region AddMetersFromList

        [HttpPost("add_meters_from_list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddMetersFromList([FromBody] AddMetersFromMeterListQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        #endregion




        #region GetMetersFromMaster
        [HttpPost("get_meters_from_unitnumber_estatename")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MetersUtilityDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMetersbyUnitNumber([FromBody] FetchMeterFromCustAggmentCommandRequest request)
        {

            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #endregion
    }
}
