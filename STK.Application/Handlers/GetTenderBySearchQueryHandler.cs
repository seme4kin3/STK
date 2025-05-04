using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetTenderBySearchQueryHandler: IRequestHandler<GetTenderBySearchQuery, PagedList<TenderDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetTenderBySearchQueryHandler> _logger;

        public GetTenderBySearchQueryHandler(DataContext dataContext, ILogger<GetTenderBySearchQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<PagedList<TenderDto>> Handle(GetTenderBySearchQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var tendersQuery = _dataContext.Tenders.AsQueryable();

                // Применяем фильтры в зависимости от наличия параметров
                if (!string.IsNullOrWhiteSpace(query.Search) && query.StartedDate.HasValue && query.EndDate.HasValue)
                {
                    // Все три параметра заданы
                    DateTime startDate = query.StartedDate.Value.Date;
                    DateTime endDate = query.EndDate.Value.Date.AddDays(1).AddTicks(-1); // Включаем весь конечный день

                    tendersQuery = tendersQuery
                        .Where(t => (t.Title.Contains(query.Search) || t.OrganizationName.Contains(query.Search))
                                 && t.PlacedDateRaw >= startDate
                                 && t.PlacedDateRaw <= endDate);
                }
                else if (!string.IsNullOrWhiteSpace(query.Search) && query.StartedDate.HasValue)
                {
                    // Только поисковая строка и начальная дата
                    DateTime startDate = query.StartedDate.Value.Date;

                    tendersQuery = tendersQuery
                        .Where(t => (t.Title.Contains(query.Search) || t.OrganizationName.Contains(query.Search))
                                 && t.PlacedDateRaw >= startDate);
                }
                else if (!string.IsNullOrWhiteSpace(query.Search) && query.EndDate.HasValue)
                {
                    // Только поисковая строка и конечная дата
                    DateTime endDate = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);

                    tendersQuery = tendersQuery
                        .Where(t => (t.Title.Contains(query.Search) || t.OrganizationName.Contains(query.Search))
                                 && t.PlacedDateRaw <= endDate);
                }
                else if (query.StartedDate.HasValue && query.EndDate.HasValue)
                {
                    // Только диапазон дат
                    DateTime startDate = query.StartedDate.Value.Date;
                    DateTime endDate = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);

                    tendersQuery = tendersQuery
                        .Where(t => t.PlacedDateRaw >= startDate && t.PlacedDateRaw <= endDate);
                }
                else if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    // Только поисковая строка
                    tendersQuery = tendersQuery
                        .Where(t => t.Title.Contains(query.Search) || t.OrganizationName.Contains(query.Search));
                }
                else if (query.StartedDate.HasValue)
                {
                    // Только начальная дата
                    DateTime startDate = query.StartedDate.Value.Date;
                    tendersQuery = tendersQuery.Where(t => t.PlacedDateRaw >= startDate);
                }
                else if (query.EndDate.HasValue)
                {
                    // Только конечная дата
                    DateTime endDate = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                    tendersQuery = tendersQuery.Where(t => t.PlacedDateRaw <= endDate);
                }

                // Сортировка по дате создания (новые сначала)
                tendersQuery = tendersQuery.OrderByDescending(t => t.PlacedDateRaw);

                // Получение общего количества для пагинации
                var totalCount = await tendersQuery.CountAsync(cancellationToken);

                // Применение пагинации и преобразование в DTO
                var items = await tendersQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
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
                        Price = t.Price
                    })
                    .ToListAsync(cancellationToken);

                return new PagedList<TenderDto>(items, totalCount, query.PageNumber, query.PageSize);
            }

            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
