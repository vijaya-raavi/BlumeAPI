using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumer;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Consumer.Commands;
using Ontec.Core.Domain.Requests.Consumer.Queries;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ConsumerController : ApiBaseController
    {
        [HttpPost("get_consumers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DatatableModel<ConsumerDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumers([FromBody] GetConsumersQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
   
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
        [HttpGet("get-consumers-masters/{estateId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ConsumerMasterDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConsumerMasters([FromRoute] int estateId)
        {
            var request = new GetConsumerMastersQuery
            {
                EstateId=estateId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        

        [HttpPost("edit-consumer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddUpdateResultDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateConsumer([FromBody] UpdateConsumerQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }


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
        [HttpDelete("remove-Consumer-from-group")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsumerGroupDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveConsumerFromGroup([FromBody] RemoveConsumerFromGroupQueryRequest request)
        {
            
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

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
