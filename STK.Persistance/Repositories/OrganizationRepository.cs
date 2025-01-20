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
                .DefaultIfEmpty(null)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
