using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class LicenseDto
    {
        public string NameTypeActivity { get; set; }
        public string SeriesNumber { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string NameOrganizationIssued { get; set; }
    }
}
