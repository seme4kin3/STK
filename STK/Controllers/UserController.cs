using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Handlers;
using System.Security.Claims;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Application.Commands;

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

        [Authorize(Roles = "admin,user")]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { Message = "ID пользователя в токене не найдено." });
                }

                var query = new GetCurrentUserQuery(userId);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (DomainException ex)
            {
                return StatusCode(ex.StatusCode, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred while retrieving user information.", Detail = ex.Message });
            }
        }

        [Authorize(Roles = "admin,user")]
        [HttpPost("decrementrequest")]
        public async Task<IActionResult> DecrementCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("ID пользователя в токене не найдено.");
            }

            var command = new DecrementUserCountRequestAiCommand(Guid.Parse(userId));
            var result = await _mediator.Send(command);
            return Ok(new { NumOfRemainingRequests = result });
        }

        [HttpPut("subscription")]
        public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionDto updateSubscription)
        {
            var command = new UpdateUserSubscriptionCommand(updateSubscription);
            var result = await _mediator.Send(command);
            return Ok(new {Message = "Подписка продлена", DateOfStopSub = result});
        }

        [Authorize(Roles = "admin,user")]
        [HttpPut("changepassword")]
        public async Task<IActionResult> ChangePasswordUser([FromBody] BaseUserDto user)
        {
            var command = new ChangeUserPasswordCommand(user);
            var result = await _mediator.Send(command);
            return Ok(new { Message = "Пароль успешно изменен"});
        }
    }
}
