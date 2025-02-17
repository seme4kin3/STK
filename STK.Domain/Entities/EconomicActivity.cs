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
        public string OKVDnumber { get; set; }
        public string Discription {  get; set; }
        public ICollection<Organization> Organization { get; set; } = new List<Organization>();

    }
}
