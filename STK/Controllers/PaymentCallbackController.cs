using MediatR;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.DTOs.TBank;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/payment-callback")]
    public class PaymentCallbackController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PaymentCallbackController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Callback([FromBody] TBankInitResponseDto payload)
        {
            var cmd = new PaymentCallbackCommand
            {
                OrderId = Guid.Parse(payload.OrderId),
                Status = payload.Status,
                Success = payload.Success,
                PaymentId = payload.PaymentId
            };

            await _mediator.Send(cmd);
            return Ok("OK");
        }
    }
}
