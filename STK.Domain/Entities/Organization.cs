using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Domain.Entities
{
    public class Organization 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Adress { get; set; }
        public string IndexAdress { get; set; }
        public Guid? ParrentOrganizationId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public Requisite Requisites { get; set; }
        public ICollection<Management> Managements { get; set; } = new List<Management>();
        public ICollection<EconomicActivity> EconomicActivities { get; set; } = new List<EconomicActivity>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    }
}
