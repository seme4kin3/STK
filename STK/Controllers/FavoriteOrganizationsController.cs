using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Domain.Entities;
using System.Security.Claims;

namespace STK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/favoritesorganizations")]
    public class FavoriteOrganizationsController : Controller
    {
        private readonly IMediator _mediator;
        public FavoriteOrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IReadOnlyList<SearchOrganizationDTO>>> GetFavoritesOrganizations(Guid userId)
        {
            var query = new GetFavoriteOrganizationQuery { UserId = userId };
            var result = await _mediator.Send(query);
            if (result == null)
            {
                return Ok(new List<object>());
            }
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToFavoriteOrganization([FromBody] FavoriteOrganization favoriteOrganization)
        {
            try
            {
                FavoriteOrganizationCommand command = new FavoriteOrganizationCommand();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }
                command.UserId = Guid.Parse(userId);
                command.OrganizationId = favoriteOrganization.OrganizationId;
                await _mediator.Send(command);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromFavoriteOrganization([FromBody] FavoriteOrganization favoriteOrganization)
        {
            try
            {
                FavoriteOrganizationCommand command = new FavoriteOrganizationCommand();
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }
                command.UserId = Guid.Parse(userId);
                command.OrganizationId = favoriteOrganization.OrganizationId;
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
