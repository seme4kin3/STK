using MediatR;
using Microsoft.AspNetCore.Mvc;
using STK.Application.Commands;
using STK.Application.Middleware;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/payment-callback")]
    public class PaymentCallbackController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PaymentCallbackController(IMediator mediator) => _mediator = mediator;

        public async Task<IActionResult> Callback([FromBody] Dictionary<string, object> payload)
        {
            try
            {
                // Получаем значения из словаря (без ошибок)
                var orderIdStr = payload.GetValueOrDefault("OrderId")?.ToString();
                var status = payload.GetValueOrDefault("Status")?.ToString();
                var success = false;
                var paymentId = payload.GetValueOrDefault("PaymentId")?.ToString();

                // Конвертация Success в bool (может быть bool, string, int)
                if (payload.TryGetValue("Success", out var successVal))
                {
                    if (successVal is bool b) success = b;
                    else if (bool.TryParse(successVal?.ToString(), out var parsed)) success = parsed;
                    else if (int.TryParse(successVal?.ToString(), out var intVal)) success = intVal != 0;
                }

                if (!Guid.TryParse(orderIdStr, out var orderId))
                    return BadRequest("OrderId is not a valid GUID");

                var cmd = new PaymentCallbackCommand
                {
                    OrderId = orderId,
                    Status = status,
                    Success = success,
                    PaymentId = paymentId
                };

                await _mediator.Send(cmd);
                return Content("OK"); // именно так для Тинькоффа
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
