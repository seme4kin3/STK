using MediatR;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;

namespace STK.API.Controllers
{
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CertificateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("certificates")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ListCertificates>>> GetAllCertificates()
        {
            var certificates = await _mediator.Send(new GetListCertificatesQuery());
            return Ok(certificates);
        }
    }
}
