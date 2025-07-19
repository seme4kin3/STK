
namespace STK.Domain.Entities
{
    public class TaxMode
    {
        public Guid Id {  get; set; }
        public string Name { get; set; }
        public ICollection<Organization> Organization { get; set; } = new List<Organization>();

    }
}
