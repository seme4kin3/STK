using MediatR;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Handlers;
using STK.Application.Queries;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/demo")]
    public class DemoController : ControllerBase
    {

        private readonly IMediator _mediator;

        public DemoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("organizations/{organizationId}")]
        public async Task<IActionResult> GetDemoOrganizationById([FromRoute] Guid organizationId)
        {
            var request = new GetOrganizationByIdQuery(organizationId, null);
            var organization = await _mediator.Send(request);

            if (organization == null)
            {
                return Ok(new List<object>());
            }
            return Ok(organization);
        }

        [HttpGet("organizations")]
        public async Task<IActionResult> GetDemoOrganizations()
        {
            var request = new GetDemoOrganizationsQuery();
            var organization = await _mediator.Send(request);

            if(organization == null)
            {
                return Ok(new List<object>());  
            }

            return Ok(organization);
        }

        [HttpGet("certificates")]
        public async Task<IActionResult> GetDemoCertificates()
        {
            var request = new GetDemoCertificatesQuery();
            var certificates = await _mediator.Send(request);

            if(certificates == null)
            {
                return Ok(new List<object>());
            }
            return Ok(certificates);
        }

        [HttpGet("tenders")]
        public async Task<IActionResult> GetDemoTenders()
        {
            var request = new GetDemoTendersQuery();
            var tenders = await _mediator.Send(request);

            if(tenders == null)
            {
                return Ok(new List<object>());
            }

            return Ok(tenders);
        }
    }
}
