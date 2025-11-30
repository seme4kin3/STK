using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetStampsBySearchQueryHandler : IRequestHandler<GetStampsBySearchQuery, PagedList<StampDto>>
    {
        private readonly DataContext _dataContext;

        public GetStampsBySearchQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<PagedList<StampDto>> Handle(GetStampsBySearchQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query.Search))
            {
                throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
            }

            if (query.PageNumber < 1 || query.PageSize < 1)
            {
                throw new ArgumentException("Page number and page size must be greater than 0.");
            }

            var searchTerm = query.Search.Trim();

            var stamps = await _dataContext.Stamps
                .Where(s => EF.Functions.ILike(s.Title!, $"%{searchTerm}%") || EF.Functions.ILike(s.StampNum!, $"%{searchTerm}%"))
                .OrderByDescending(s => s.Registration)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new StampDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    StampNum = s.StampNum,
                    StampStatus = s.StampStatus,
                    Contragent = s.Contragent,
                    Place = s.Place,
                    Status = s.Status,
                    Registration = s.Registration,
                    Validity = s.Validity,
                    Usage = s.Usage,
                    OrganizationId = s.OrganizationId
                })
                .ToListAsync(cancellationToken);

            return new PagedList<StampDto>(stamps, stamps.Count, query.PageNumber, query.PageSize);
        }
    }
}
