
namespace STK.Application.DTOs
{
    public class TenderDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string PlatformLink { get; set; }
        public string PlatformHeader { get; set; }
        public DateTime PlacedDateRaw { get; set; }
        public DateTime EndDateStr { get; set; }
        public string OrganizationName { get; set; }
        public decimal Price { get; set; }
    }
}
