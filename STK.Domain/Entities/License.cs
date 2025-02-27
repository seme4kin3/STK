using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Domain.Entities
{
    public class License
    {
        public Guid Id { get; set; }
        public string SeriesNumber { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string NameOrganizationIssued { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
