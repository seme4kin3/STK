using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using System.Security.Claims;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //[Authorize(Roles = "admin")]
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            //var command = new RegisterUserCommand { Register = registerDto };
            //var result = await _mediator.Send(command);

            //return CreatedAtAction(nameof(Register), result);

            try
            {
                var command = new RegisterUserCommand(registerDto);
                var userId = await _mediator.Send(command);
                return Ok(new { UserEmail = userId }); // 200 OK
            }
            catch (DomainException ex)
            {
                return StatusCode(ex.StatusCode, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred during registration.", Detail = ex.Message }); // 400 Bad Request
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto authDto)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            //var command = new AuthenticateUserCommand { AuthDto = authDto };
            //var result = await _mediator.Send(command);
            //if(result == null)
            //{
            //    return Unauthorized(new
            //    {
            //        message = "Неверный логин или пароль. Пожалуйста, проверьте введенные данные и попробуйте снова."
            //    });
            //}

            //return Ok(result);
            try
            {
                var command = new AuthenticateUserCommand(authDto);
                var response = await _mediator.Send(command);
                return Ok(response); // 200 OK
            }
            catch (DomainException ex)
            {
                return StatusCode(ex.StatusCode, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred during authentication.", Detail = ex.Message }); // 400 Bad Request
            }
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshTokenRequest)
        {
            var command = new RefreshTokenCommand { RefreshTokenRequest = refreshTokenRequest };
            var result = await _mediator.Send(command);

            return Ok(new {result.AccessToken, result.RefreshToken});
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user in token.");
            }

            try
            {
                // Создаем команду для выхода из системы
                var command = new LogoutCommand { UserId = userId };

                // Выполняем команду
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (DomainException ex)
            {
                return StatusCode(ex.StatusCode, new { Message = ex.Message });
            }

        }
    }
}
