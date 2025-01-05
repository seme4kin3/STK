using Microsoft.EntityFrameworkCore;
using STK.Domain.Entities;
using STK.Persistance.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Organization> GetOrganizationById(int id)
        {
            return await _dataContext.Organizations.FindAsync(id);
        }
    }
}
