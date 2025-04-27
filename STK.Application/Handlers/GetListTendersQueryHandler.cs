using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetListTendersQueryHandler : IRequestHandler<GetListTendersQuery, IReadOnlyList<TenderDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetListTendersQueryHandler> _logger;

        public GetListTendersQueryHandler(DataContext dataContext, ILogger<GetListTendersQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<TenderDto>> Handle(GetListTendersQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var tenders = await _dataContext.Tenders
                    .AsNoTracking()
                    .OrderByDescending(t => t.PlacedDateRaw)
                    .Take(50)
                    .Select(t => new TenderDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Url = t.Url,
                        PlatformLink = t.PlatformLink,
                        PlatformHeader = t.PlatformHeader,
                        PlacedDateRaw = t.PlacedDateRaw,
                        EndDateStr = t.EndDateStr,
                        OrganizationName = t.OrganizationName,
                        Price = t.Price,
                    })
                    .ToListAsync(cancellationToken);

                if (tenders == null)
                {
                    return null;
                }

                return tenders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
