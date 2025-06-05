using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Queries;

namespace STK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/predictai")]
    public class PredictAiController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PredictAiController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _mediator.Send(new GetAllPredictAisQuery());
            return Ok(results);
        }
    }
}
