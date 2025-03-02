using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Domain.Entities
{
    public class OrganizationEconomicActivity
    {
        public Guid OrganizationId { get; set; }
        public Guid EconomicActivityId { get; set; }
        public bool IsMain { get; set; }
        public Organization Organizations { get; set; }
        public EconomicActivity EconomicActivities { get; set; }
    }
}
