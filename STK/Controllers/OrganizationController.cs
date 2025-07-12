using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace STK.API.Controllers
{

    [Route("api/")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [Authorize]
        [Route("organizations")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SearchOrganizationDTO>>> GetAllOrganizations([FromQuery] bool? isNew, [FromQuery] bool? isChange)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var query = new GetOrganizationsQuery(userId, isNew, isChange);
            
            var organizations = await _mediator.Send(query);
            return Ok(organizations);
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
        [HttpGet("organizations/organizationchanges/{organizationId}")]
        public async Task<IActionResult> GetOrganizationChanges(Guid organizationId)
        {
            var query = new GetOrganizationChangesQuery(organizationId);

            var changes = await _mediator.Send(query);
            return Ok(changes);
        }

        [Authorize]
        [HttpGet("organizations/arbitration/{organizationId}")]
        public async Task<IActionResult> GetArbitrationOfOrganization(Guid organizationId)
        {
            var query = new GetArbitrationCaseQuery(organizationId);

            var arbitration = await _mediator.Send(query);
            if(arbitration == null || !arbitration.Any())
            {
                return Ok(new List<object>());
            }
            return Ok(arbitration);
        }
    }
    
}
