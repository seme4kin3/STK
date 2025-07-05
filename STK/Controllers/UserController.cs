using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Handlers;
using System.Security.Claims;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost("{userId}/decrementrequest")]
        public async Task<IActionResult> DecrementCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var command = new DecrementUserCountRequestAiCommand(Guid.Parse(userId));
            var result = await _mediator.Send(command);
            return Ok(new { NumOfRemainingRequests = result });
        }

        [HttpPut("{userId}/subscription")]
        public async Task<IActionResult> UpdateSubscription([FromBody] string userEmail, [FromBody] string subscriptionType)
        {
            var command = new UpdateUserSubscriptionCommand(userEmail, subscriptionType);
            var result = await _mediator.Send(command);
            return Ok(new {Message = "Подписка продлена", DateOfStopSub = result});
        }
    }
}
