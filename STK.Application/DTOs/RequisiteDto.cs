using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class RequisiteDto
    {
        public string INN { get; set; }
        public string KPP { get; set; }
        public string OGRN { get; set; }
        public DateTime? DateCreation { get; set; }
        public string EstablishmentCreateName { get; set; }
        public int? AuthorizedCapital { get; set; }
        public int? AvgCountEmployee { get; set; }
        public string TypeOfCapital { get; set; }
    }
}
