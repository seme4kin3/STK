using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using System.Security.Claims;
using System.Text.Json;

namespace STK.API.Controllers
{
    [ApiController]
    [Route("api/")]
    [Authorize(Roles = "admin,user")]
    public class StampController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StampController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("stamps")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StampDto>>> GetStamps()
        {
            var query = new GetLatestStampsQuery();
            var stamps = await _mediator.Send(query);

            if (stamps == null || !stamps.Any())
            {
                return Ok(new List<object>());
            }

            return Ok(stamps);
        }


        [Route("stamps/search/")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StampDto>>> GetStampsBySearch(
            [FromQuery] string text, int page = 1, int limit = 20)
        {

            var query = new GetStampsBySearchQuery(text, page, limit);
            var stamps = await _mediator.Send(query);

            if (stamps == null || !stamps.Any())
            {
                return Ok(new List<object>());
            }

            var metadata = new
            {
                totalCount = stamps.TotalCount,
                limit = stamps.PageSize,
                currentPage = stamps.CurrentPage,
                totalPages = stamps.TotalPages,
                hasNext = stamps.HasNext,
                hasPrevious = stamps.HasPrevious,
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
            return Ok(stamps);
        }
    }
}
