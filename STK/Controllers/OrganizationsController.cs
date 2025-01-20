using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.DTOs.ListOrganizations;
using STK.Application.Queries;

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
        public async Task<ActionResult<List<ConciseOrganizationsDto>>> GetAllOrganizations()
        {
            var organizations = await _mediator.Send(new GetOrganizationsQuery());
            return Ok(organizations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganizationById(Guid id)
        {
            var request = new GetOrganizationByIdQuery(id);
            var organization = await _mediator.Send(request);
            if(organization == null)
            {
                return NotFound();
            }   
            return Ok(organization);
        }
    }
    
}
