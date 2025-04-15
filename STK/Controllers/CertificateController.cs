using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using System.Text.Json;
using System.Security.Claims;

namespace STK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/")]
    public class CertificateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CertificateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("certificates")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SearchCertificatesDto>>> GetAllCertificates()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var query = new GetListCertificatesQuery(userId);
            var certificates = await _mediator.Send(query);
            return Ok(certificates);
        }

        [Route("certificates/search/")]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SearchCertificatesDto>>> GetCertificatesByObject([FromQuery] string text, int page = 1, int limit = 20)
        {
            var query = new GetCertificateBySearchQuery(text, page, limit);
            var certificates = await _mediator.Send(query);

            if (certificates == null || !certificates.Any())
            {
                return Ok(new List<object>());
            }

            var metadata = new
            {
                totalCount = certificates.TotalCount,
                limit = certificates.PageSize,
                currentPage = certificates.CurrentPage,
                totalPages = certificates.TotalPages,
                hasNext = certificates.HasNext,
                hasPrevious = certificates.HasPrevious,
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
            return Ok(certificates);
        }
    }
}
