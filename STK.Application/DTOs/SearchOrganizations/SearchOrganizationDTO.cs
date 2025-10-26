
namespace STK.Application.DTOs.SearchOrganizations
{
    public class SearchOrganizationDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Inn { get; set; }
        public string Ogrn { get; set; }
        public string Kpp { get; set; }
        public bool IsFavorite { get; set; }
        public bool? AddressBool { get; set; }
        public string StatusChange { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? AddressAdded { get; set; }
        public List<SearchManagementDTO> Managements { get; set; }
        public List<SearchEconomicActivityDto> SearchEconomicActivities { get; set; }
    }
}
