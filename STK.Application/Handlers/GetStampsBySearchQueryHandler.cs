using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetStampsBySearchQueryHandler
        : IRequestHandler<GetStampsBySearchQuery, PagedList<StampDto>>
    {
        private readonly DataContext _dataContext;

        public GetStampsBySearchQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<PagedList<StampDto>> Handle(GetStampsBySearchQuery query, CancellationToken cancellationToken)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            if (string.IsNullOrWhiteSpace(query.Search))
            {
                throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
            }

            if (query.PageNumber < 1 || query.PageSize < 1)
            {
                throw new ArgumentException("Page number and page size must be greater than 0.");
            }

            var searchTerm = query.Search.Trim();
            var pattern = $"%{searchTerm}%";

            var baseQuery = _dataContext.Stamps
                .AsNoTracking()
                .Where(s =>
                    EF.Functions.ILike(s.Title ?? string.Empty, pattern) ||
                    EF.Functions.ILike(s.StampNum ?? string.Empty, pattern));

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            var stamps = await baseQuery
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

            return new PagedList<StampDto>(stamps, totalCount, query.PageNumber, query.PageSize);
        }
    }

}
