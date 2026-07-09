using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumer;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Consumer.Commands;
using Ontec.Core.Domain.Requests.Consumer.Queries;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ConsumerController : ApiBaseController
    {
        [MapToApiVersion("2.0")]
        [HttpPost("get_consumers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DatatableModel<ConsumerDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumers([FromBody] GetConsumersQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get_consumers_dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsumerDashboardDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumersDashboard()
        {
            var request = new GetConsumerDashboardQuery
            {
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpGet("get-consumers-masters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsumerMasterDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumerMasters()
        {
            var request = new GetConsumerMastersQuery
            {
                //EstateId=estateId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [MapToApiVersion("2.0")]
        [HttpPost("edit-consumer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateConsumer([FromBody] UpdateConsumerQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [MapToApiVersion("2.0")]
        [HttpDelete("delete-Consumer-user/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteConsumerbyId([FromRoute] int id)
        {
            var request = new DeleteConsumerById
            {
                Id = id
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #region ForApproveRejectUserRequest
        [MapToApiVersion("2.0")]
        [HttpPost("registration-approve-reject-by-id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveRejectRegistrtionRequest([FromBody] ApproveRejectRegistrtionRequestQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #endregion
        [MapToApiVersion("2.0")]
        [HttpGet("get-Consumer-groups/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsumerGroupDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumerGroups([FromRoute] int id)
        {
            var request = new GetConsumerGroupQueryRequest
            {
                UserId = id
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpDelete("remove-Consumer-from-group")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsumerGroupDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveConsumerFromGroup([FromBody] RemoveConsumerFromGroupQueryRequest request)
        {
            
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [HttpPost("send-notification-to-consumer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PushNotificationDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendNotificationtoConsumer([FromBody] SendNotifcationToConsumerRequest request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

    }
}
