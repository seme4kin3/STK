using STK.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Adress { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string StatusOrg { get; set; }
        public Requisite Requisites { get; set; }
        public List<Management> Management { get; set; } = new List<Management>();
        public List<EconomicActivity> EconomicActivities { get; set; } = new List<EconomicActivity> { };
        public List<Certificate> Certificate { get; set; } = new List<Certificate> { };
    }
}
