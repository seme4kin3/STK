using STK.Application.DTOs.SearchOrganizations;


namespace STK.Application.DTOs
{
    public class UserCreatedOrganizationsResultDto
    {
        public List<SearchOrganizationDTO> Organizations { get; init; } = new List<SearchOrganizationDTO>();
        public List<string> NotLoadedInns { get; init; } = new List<string>();
    }
}
