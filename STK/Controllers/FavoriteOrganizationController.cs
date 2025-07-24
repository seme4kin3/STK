using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using System.Security.Claims;

namespace STK.API.Controllers
{
    [Authorize(Roles = "user, admin")]
    [ApiController]
    [Route("api/favoritesorganizations")]
    public class FavoriteOrganizationController : Controller
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
