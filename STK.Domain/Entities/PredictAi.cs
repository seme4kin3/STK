using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Domain.Entities
{
    public class PredictAi
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public string EconNews { get; set; }
        public string SegmentNews { get; set; }
        public string MarketData { get; set; }
        public string FinalPredict {  get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
