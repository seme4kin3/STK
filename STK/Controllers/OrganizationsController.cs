using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using System.Text.Json;

namespace STK.API.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationsController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [Route("/organizations")]
        [HttpGet]
        public async Task<ActionResult<List<SearchOrganizationDTO>>> GetAllOrganizations()
        {
            var organizations = await _mediator.Send(new GetOrganizationsQuery());
            return Ok(organizations);
        }

        [HttpGet("/organizations/{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganizationById(Guid id)
        {
            var request = new GetOrganizationByIdQuery(id);
            var organization = await _mediator.Send(request);

            if (organization == null)
            {
                return NotFound();
            }
            return Ok(organization);
        }
        
        [HttpGet]
        public async Task<ActionResult<List<SearchOrganizationDTO>>> Search([FromQuery] string search, int pageNumber = 1, int pageSize = 20)
            
        {
            var query = new GetOrganizationBySearchQuery(search, pageNumber, pageSize);
            var organizations = await _mediator.Send(query);

            if (organizations == null || !organizations.Any())
            {
                return NotFound("No organizations found.");
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
    }
    
}
