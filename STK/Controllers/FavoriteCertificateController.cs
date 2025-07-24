using MediatR;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.DTOs;
using STK.Application.Queries;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace STK.API.Controllers
{
    [Authorize(Roles = "user, admin")]
    [ApiController]
    [Route("api/favoritescertificates")]
    public class FavoriteCertificateController : Controller
    {
        private readonly IMediator _mediator;

        public FavoriteCertificateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SearchCertificatesDto>>> GetFavoritesCertificates()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            var query = new GetFavoriteCertificateQuery { UserId = Guid.Parse(userId) };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return Ok(new List<object>());
            }
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddFavoriteCertificate([FromBody] FavoriteDto certificateDto)
        {
            try
            {
                AddFavoriteCertificateCommand command = new AddFavoriteCertificateCommand();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }
                command.UserId = Guid.Parse(userId);
                command.CertificateId = certificateDto.Id;
                await _mediator.Send(command);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromFavoriteOrganization([FromBody] FavoriteDto certificateDto)
        {
            try
            {
                RemoveFavoriteCertificateCommand command = new RemoveFavoriteCertificateCommand();
  
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }
                command.UserId = Guid.Parse(userId);
                command.CertificateId = certificateDto.Id;
                await _mediator.Send(command);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
