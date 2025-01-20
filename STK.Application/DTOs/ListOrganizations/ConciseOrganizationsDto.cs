using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs.ListOrganizations
{
    public class ConciseOrganizationsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Adress { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public ConciseRequisiteDto ConciseRequisite { get; set; }
        public List<EconomicActivityDto> EconomicActivities { get; set; }
    }
}
