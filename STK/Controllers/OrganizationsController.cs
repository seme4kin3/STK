using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using System.Text.Json;

namespace STK.API.Controllers
{
    [Route("api/organizations")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationsController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<SearchOrganizationDTO>>> GetAllOrganizations()
        {
            var organizations = await _mediator.Send(new GetOrganizationsQuery());
            return Ok(organizations);
        }

        [HttpGet("{id}")]
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

        [HttpGet("search")]
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
                organizations.TotalCount,
                organizations.PageSize,
                organizations.CurrentPage,
                organizations.TotalPages,
                organizations.HasNext,
                organizations.HasPrevious,
            };
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
            return Ok(organizations);
        }
    }
    
}
