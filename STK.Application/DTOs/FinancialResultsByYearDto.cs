using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class FinancialResultsByYearDto
    {
        public int Year { get; set; }
        public List<FinancialResultDto> Results { get; set; }
    }
}
