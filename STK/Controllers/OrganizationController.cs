using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Handlers;
using STK.Application.Middleware;
using STK.Application.Queries;
using System.Security.Claims;
using System.Text.Json;

namespace STK.API.Controllers
{
    
    [ApiController]
    [Route("api/")]
    [Authorize(Roles = "admin,user")]
    public class OrganizationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [Route("organizations")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SearchOrganizationDTO>>> GetAllOrganizations([FromQuery] bool isNew = true)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var query = new GetOrganizationsQuery(userId, isNew);
            
            var organizations = await _mediator.Send(query);
            return Ok(organizations);
        }


        [HttpPost("organizations/create")]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationDto dto, CancellationToken ct)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized("User ID not found in token.");

                var cmd = new CreateOrganizationCommand(dto, Guid.Parse(userId));

                await _mediator.Send(cmd, ct);

                return NoContent();
            }
            catch(DomainException ex)
            {
                return StatusCode(ex.StatusCode, new { Message = ex.Message });
            }
        }

        [Route("organizations/created")]
        [HttpGet]
        public async Task<ActionResult<OrganizationDto>> GetCreatedOrganization()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            var query = new GetUserCreatedOrganizationsQuery { UserId = Guid.Parse(userId) };

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("organizations/{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganizationById([FromRoute] Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var request = new GetOrganizationByIdQuery(id, userId);
            var organization = await _mediator.Send(request);

            if (organization == null)
            {
                return Ok(new List<object>());
            }
            return Ok(organization);
        }

        [HttpGet("organizations/search/")]
        public async Task<ActionResult<List<SearchOrganizationDTO>>> Search([FromQuery] string text, int page = 1, int limit = 20)    
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var query = new GetOrganizationBySearchQuery(text, page, limit, userId);
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

        [HttpGet("organizations/organizationchanges/{organizationId}")]
        public async Task<IActionResult> GetOrganizationChanges(Guid organizationId)
        {
            var query = new GetOrganizationChangesQuery(organizationId);

            var changes = await _mediator.Send(query);
            return Ok(changes);
        }

        [HttpGet("organizations/arbitration/{organizationId}")]
        public async Task<IActionResult> GetArbitrationOfOrganization(
            Guid organizationId,
            [FromQuery] ArbitrationPartyRole role = ArbitrationPartyRole.Any)
        {
            var query = new GetArbitrationCaseQuery(organizationId, role);

            var arbitration = await _mediator.Send(query);
            if (arbitration == null || !arbitration.Any())
            {
                return Ok(new List<object>());
            }

            return Ok(arbitration);
        }
    }
    
}
