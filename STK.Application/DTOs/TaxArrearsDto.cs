using STK.Domain.Entities;


namespace STK.Application.DTOs
{
    public class TaxArrearsDto
    {
        public decimal Amount { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public bool IsPayOff { get; set; }
    }
}
