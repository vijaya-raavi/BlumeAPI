using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.BankNotification;
using Ontec.Notification.Api.Models;
using Ontec.Notification.Core.Interfaces;

namespace Ontec.Notification.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/notification")]
    public class NotificationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly INotificationProcessor _notificationProcessor;

        public NotificationController(IMapper mapper, INotificationProcessor notificationProcessor)
        {
            _mapper = mapper;
            _notificationProcessor = notificationProcessor;
        }
        [HttpPost]
        [Route("receive")]
        public async Task<IActionResult> ReceiveNotification([FromBody] NotificationRequest notificationRequest)
        {
            await _notificationProcessor.ProcessAsync(_mapper.Map<BankNotificationDetails>(notificationRequest));
            return Ok();
        }
    }
}
