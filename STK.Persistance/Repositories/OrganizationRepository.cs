using Microsoft.EntityFrameworkCore;
using STK.Domain.Entities;
using STK.Persistance.Interfaces;



namespace STK.Persistance.Repositories
{
    public class OrganizationRepository: IOrganizationRepository
    {
        private readonly DataContext _dataContext;

        public OrganizationRepository(DataContext dataContext) 
        { 
            _dataContext = dataContext;
        }

        public async Task<List<Organization>> GetAllOrganizations()
        {
            return await _dataContext.Organizations
                .Include(o => o.Requisites)
                .Include(o => o.EconomicActivities)
                .ToListAsync();
        }

        public async Task<Organization?> GetOrganizationById(Guid id)
        {
            return await _dataContext.Organizations
                .Include(o => o.Requisites)
                .Include(o => o.EconomicActivities)
                .Include(o => o.Managements)
                .Include(o => o.Certificates)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Organization>> GetOrganizationBySearch(string search)
        {
            var allowedCodes = new List<string> { "30.20.9", "30.20.31", "52.21.1" };

            var organizations = await _dataContext.Organizations
                .Where(o => o.Name.Contains(search) ||
                            o.FullName.Contains(search) ||
                            o.Requisites.INN.StartsWith(search) ||
                            o.Requisites.OGRN.StartsWith(search))
                .Where(o => o.EconomicActivities.Any(e => allowedCodes.Contains(e.OKVDNnumber))) // Фильтр организаций
                .Select(o => new Organization
                {
                    Id = o.Id,
                    Name = o.Name,
                    FullName = o.FullName,
                    Adress = o.Adress,
                    IndexAdress = o.IndexAdress,
                    Requisites = o.Requisites,
                    Managements = o.Managements,
                    EconomicActivities = o.EconomicActivities
                        .Where(e => allowedCodes.Contains(e.OKVDNnumber)) // Фильтр видов деятельности
                        .ToList()
                })
                .ToListAsync();

            return organizations;
        }
    }
}
