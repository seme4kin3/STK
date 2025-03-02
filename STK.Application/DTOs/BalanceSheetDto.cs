using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class BalanceSheetDto
    {
        public int Year { get; set; }
        public string AssetType { get; set; }
        public decimal? NonCurrentActive { get; set; }
        public decimal? CurrentActive { get; set; }
        public decimal? CapitalReserves { get; set; }
        public decimal? LongTermLiabilities { get; set; }
        public decimal? ShortTermLiabilities { get; set; }
    }
}
