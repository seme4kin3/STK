using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.Queries;

namespace STK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationsController(IMediator mediator) 
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<ActionResult<List<OrganizationDto>>> GetAllOrganizations()
        {
            var organizations = await _mediator.Send(new GetOrganizationsQuery());
            return Ok(organizations);
        }
    }
    
}
