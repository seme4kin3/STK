using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Middleware;
using STK.Application.Queries;
using STK.Domain.Entities;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/subscription-prices")]
    public class SubscriptionPriceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionPriceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory([FromQuery] SubscriptionPriceCategory category, CancellationToken cancellationToken)
        {
            try
            {
                var prices = await _mediator.Send(new GetSubscriptionPricesByCategoryQuery { Category = category }, cancellationToken);
                return Ok(prices);
            }
            catch (DomainException ex)
            {
                return StatusCode(ex.StatusCode, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
