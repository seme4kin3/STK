using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetOrganizationBySearchHandler : IRequestHandler<GetOrganizationBySearchQuery, PagedList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;

        public GetOrganizationBySearchHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<PagedList<SearchOrganizationDTO>> Handle(GetOrganizationBySearchQuery query, CancellationToken cancellationToken)
        {
            var allowedCodes = new List<string> { "30.20.9", "30.20.31", "52.21.1" };

            if (string.IsNullOrWhiteSpace(query.Search))
            {
                throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
            }

            //var organizations = await _dataContext.Organizations
            //    .AsNoTracking() // Отключаем отслеживание изменений
            //    .Include(o => o.Requisites)
            //    .Include(o => o.Managements)
            //    .Include(o => o.EconomicActivities)
            //    .Where(o => o.Name.Contains(query.Search) ||
            //                o.FullName.Contains(query.Search) ||
            //                o.Requisites.INN.StartsWith(query.Search) ||
            //                o.Requisites.OGRN.StartsWith(query.Search))
            //    .Where(o => o.EconomicActivities.Any(e => allowedCodes.Contains(e.OKVDNnumber))) // Фильтр организаций
            //    .Select(o => new SearchOrganizationDTO
            //    {
            //        Id = o.Id,
            //        Name = o.Name,
            //        FullName = o.FullName,
            //        Adress = o.Adress + o.IndexAdress,
            //        Inn = o.Requisites.INN,
            //        Ogrn = o.Requisites.OGRN,
            //        Kpp = o.Requisites.KPP,
            //        Managements = o.Managements.Select(m => new SearchManagementDTO
            //        {
            //            FullName = m.FullName,
            //            Position = m.Position,
            //        }).ToList(),
            //        EconomicActivities = o.EconomicActivities
            //            .Where(e => allowedCodes.Contains(e.OKVDNnumber)) // Фильтр видов деятельности
            //            .ToList()
            //    }).ToListAsync();

            //return PagedList<SearchOrganizationDTO>.ToPagedList(organizations, query.PageNumber, query.PageSize);
            var organizationsQuery = _dataContext.Organizations
                .AsNoTracking() // Отключаем отслеживание изменений
                .Include(o => o.Requisites)
                .Include(o => o.Managements)
                .Include(o => o.EconomicActivities)
                .Where(o => o.Name.Contains(query.Search) ||
                            o.FullName.Contains(query.Search) ||
                            o.Requisites.INN.StartsWith(query.Search) ||
                            o.Requisites.OGRN.StartsWith(query.Search))
                .Where(o => o.EconomicActivities.Any(e => allowedCodes.Contains(e.OKVDNnumber))); // Фильтр организаций

            // Получаем общее количество организаций
            var count = await organizationsQuery.CountAsync(cancellationToken);

            // Выполняем выбор элементов с применением проекции
            var items = organizationsQuery.Select(o => new SearchOrganizationDTO
            {
                Id = o.Id,
                Name = o.Name,
                FullName = o.FullName,
                Adress = o.Adress + o.IndexAdress, // Корректировка для аддреса
                Inn = o.Requisites.INN,
                Ogrn = o.Requisites.OGRN,
                Kpp = o.Requisites.KPP,
                Managements = o.Managements.Select(m => new SearchManagementDTO
                {
                    FullName = m.FullName,
                    Position = m.Position,
                }).ToList(), // Список менеджментов
                EconomicActivities = o.EconomicActivities
                    .Where(e => allowedCodes.Contains(e.OKVDNnumber)) // Фильтр видов деятельности
                    .ToList() // Список типов экономической деятельности
            });

            // Применяем пагинацию и выполняем запрос
            var pagedItems = await items.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return new PagedList<SearchOrganizationDTO>(pagedItems, count, query.PageNumber, query.PageSize);
        }
    }
}
