using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Domain.Entities;
using System.Text.Json;

namespace STK.API.Controllers
{
    [Authorize(Roles = "user, admin")]
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

        [Route("tenders/search")]
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? text = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            int pageNumber = 1, int pageSize = 10)
        {
            var query = new GetTenderBySearchQuery(text, pageNumber, pageSize, startDate, endDate);
            var result = await _mediator.Send(query);


            if (result == null || !result.Any())
            {
                return Ok(new List<object>());
            }

            var metadata = new
            {
                totalCount = result.TotalCount,
                limit = result.PageSize,
                currentPage = result.CurrentPage,
                totalPages = result.TotalPages,
                hasNext = result.HasNext,
                hasPrevious = result.HasPrevious,
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
            return Ok(result);
        }
    }
}
