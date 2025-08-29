using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using System.Security.Claims;
using System.Text.Json;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/favoritesorganizations")]
    [Authorize(Roles = "admin,user")]

    public class FavoriteOrganizationController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FavoriteOrganizationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SearchOrganizationDTO>>> GetFavoritesOrganizations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            var query = new GetFavoriteOrganizationQuery { UserId = Guid.Parse(userId) };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return Ok(new List<object>());
            }
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<SearchOrganizationDTO>>> SearchFavorites([FromQuery] string text, int page = 1, int limit = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var query = new GetFavoriteOrganizationBySearchQuery(text, page, limit, Guid.Parse(userId));
            var organizations = await _mediator.Send(query);

            if (organizations == null || !organizations.Any())
            {
                return Ok(new List<object>());
            }

            var metadata = new
            {
                totalCount = organizations.TotalCount,
                limit = organizations.PageSize,
                currentPage = organizations.CurrentPage,
                totalPages = organizations.TotalPages,
                hasNext = organizations.HasNext,
                hasPrevious = organizations.HasPrevious,
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
            return Ok(organizations);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToFavoriteOrganization([FromBody] FavoriteDto favoriteOrganization)
        {
            try
            {
                AddFavoriteOrganizationCommand command = new AddFavoriteOrganizationCommand();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }
                command.UserId = Guid.Parse(userId);
                command.OrganizationId = favoriteOrganization.Id;
                await _mediator.Send(command);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromFavoriteOrganization([FromBody] FavoriteDto favoriteOrganization)
        {
            try
            {
                RemoveFavoriteOrganizationCommand command = new RemoveFavoriteOrganizationCommand();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }
                command.UserId = Guid.Parse(userId);
                command.OrganizationId = favoriteOrganization.Id;
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
