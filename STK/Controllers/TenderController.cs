using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.Queries;

namespace STK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/")]
    public class TenderController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TenderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("tenders")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TenderDto>>> GetAllTenders()
        {
            var query = new GetListTendersQuery();
            var certificates = await _mediator.Send(query);
            if(certificates == null)
            {
                return Ok(new List<object>());
            }
            return Ok(certificates);
        }

    }
}
