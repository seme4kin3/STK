using STK.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs.SearchOrganizations
{
    public class SearchOrganizationDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Inn { get; set; }
        public string Ogrn { get; set; }
        public string Kpp { get; set; }
        public List<SearchManagementDTO> Managements { get; set; }
        public List<EconomicActivity> EconomicActivities { get; set; }
    }
}
