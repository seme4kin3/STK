using MediatR;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using System.ComponentModel.DataAnnotations;


namespace STK.Application.Queries
{
    public class GetCertificateBySearchQuery : IRequest<PagedList<SearchCertificatesDto>>
    {
        [Required(ErrorMessage = "Search term is required.")]
        public string Search { get; set; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public GetCertificateBySearchQuery(string search, int pageNumber, int pageSize) 
        {
            Search = search;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
