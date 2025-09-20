
namespace STK.Application.DTOs
{
    public class CreateOrganizationDto
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string IndexAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public CreateRequisiteDto RequisiteDto { get; set; } = new CreateRequisiteDto();
    }

    public class CreateRequisiteDto
    {
        public string INN { get; set; }
        public string KPP { get; set; }
        public string OGRN { get; set; }
    }
}
