using MediatR;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.DTOs;
using STK.Application.Queries;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;


namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/favoritescertificates")]
    [Authorize(Roles = "admin,user")]
    public class FavoriteCertificateController : ControllerBase
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

        [HttpGet("search")]
        public async Task<ActionResult<List<SearchCertificatesDto>>> SearchFavorites([FromQuery] string text, int page = 1, int limit = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var query = new GetFavoriteCertificateBySearchQuery(text, page, limit, Guid.Parse(userId));
            var certificates = await _mediator.Send(query);

            if (certificates == null || !certificates.Any())
            {
                return Ok(new List<object>());
            }

            var metadata = new
            {
                totalCount = certificates.TotalCount,
                limit = certificates.PageSize,
                currentPage = certificates.CurrentPage,
                totalPages = certificates.TotalPages,
                hasNext = certificates.HasNext,
                hasPrevious = certificates.HasPrevious,
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
            return Ok(certificates);
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
