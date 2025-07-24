using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.Queries;
using System.Security.Claims;

namespace STK.API.Controllers
{
    public class NotificationController : Controller
    {
        [Authorize(Roles = "user, admin")]
        [ApiController]
        [Route("api/[controller]")]
        public class NotificationsController : ControllerBase
        {
            private readonly IMediator _mediator;
            private readonly ILogger<NotificationsController> _logger;

            public NotificationsController(
                IMediator mediator,
                ILogger<NotificationsController> logger)
            {
                _mediator = mediator;
                _logger = logger;
            }

            [HttpGet]
            public async Task<IActionResult> GetUserNotifications()
            {
                var userId = GetCurrentUserId();
                var notifications = await _mediator.Send(new GetUserNotificationsQuery(userId));
                return Ok(notifications);
            }

            [HttpPost("{notificationId}/mark-as-read")]
            public async Task<IActionResult> MarkAsRead(Guid notificationId)
            {
                var userId = GetCurrentUserId();
                await _mediator.Send(new MarkNotificationAsReadCommand(notificationId, userId));
                return NoContent();
            }

            private Guid GetCurrentUserId()
            {
                return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
        }
    }
}
