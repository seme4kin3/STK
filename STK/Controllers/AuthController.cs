using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using STK.Application.Commands;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Application.Queries;
using STK.Application.Services;
using System.Security.Claims;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IEmailService _emailService;
        public AuthController(IMediator mediator, IEmailService emailService)
        {
            _mediator = mediator;
            _emailService = emailService;
        }

        //[Authorize(Roles = "admin")]
        [AllowAnonymous]
        [HttpPost("register")]
        [EnableRateLimiting("RegisterPolicy")]

        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var command = new RegisterUserCommand(registerDto);
                var result = await _mediator.Send(command);
                return Ok(new { PaymentUrl = result }); // 200 OK
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
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] BaseUserDto authDto)
        {
            try
            {
                var command = new AuthenticateUserCommand(authDto);
                var response = await _mediator.Send(command);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };

                Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

                // Не возвращать refresh token в response body
                return Ok(new
                {
                    AccessToken = response.AccessToken,
                    UserInfo = response.UserInfo
                });
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

        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Refresh token отсутствует" });
            }

            try
            {

                var command = new RefreshTokenCommand(refreshToken);
                var result = await _mediator.Send(command);
                SetRefreshTokenCookie(result.RefreshToken);

                return Ok(new { AccessToken = result.AccessToken });
            }
            catch (DomainException ex)
            {
                return StatusCode(ex.StatusCode, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred during token refresh.", Detail = ex.Message });
            }
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

        [HttpGet("payment-url/{userId}")]
        public async Task<IActionResult> GetPaymentUrl(Guid userId)
        {
            var url = await _mediator.Send(new GetPaymentUrlQuery { UserId = userId });
            return Ok(new { url });
        }

        //[HttpPost("test-email")]
        //public async Task<IActionResult> TestEmail([FromQuery] string request)
        //{
        //    try
        //    {
        //        var emailContent = new EmailContent
        //        {
        //            To = request,
        //            Subject = "Тестовое письмо",
        //            Body = $"<h2>Это тестовое письмо</h2><p>Отправлено в {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>",
        //            IsHtml = true
        //        };

        //        await _emailService.SendEmailAsync(emailContent);
        //        return Ok(new { message = "Тестовое письмо успешно отправлено" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = $"Ошибка отправки: {ex.Message}" });
        //    }
        //}

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
