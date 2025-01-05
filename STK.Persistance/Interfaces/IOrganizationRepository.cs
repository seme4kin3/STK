using STK.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Persistance.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<List<Organization>> GetAllOrganizations();
        Task<Organization> GetOrganizationById(int id);
    }
}
