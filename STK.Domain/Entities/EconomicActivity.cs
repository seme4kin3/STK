using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Domain.Entities
{
    public class EconomicActivity
    {
        public Guid Id { get; set; }
        public string OKVDNnumber { get; set; }
        public string Discription {  get; set; }
        
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

    }
}
