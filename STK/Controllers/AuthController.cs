using MediatR;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var command = new RegisterUserCommand { Register = registerDto };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto authDto)
        {
            var command = new AuthenticateUserCommand { AuthDto = authDto };
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] AuthTokenDto refreshTokenRequest)
        {
            var command = new RefreshTokenCommand { RefreshTokenRequest = refreshTokenRequest };
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
