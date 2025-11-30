using MediatR;
using STK.Application.DTOs;
using STK.Application.Pagination;
using System.ComponentModel.DataAnnotations;


namespace STK.Application.Queries
{
    public class GetStampsBySearchQuery : IRequest<PagedList<StampDto>>
    {
        [Required(ErrorMessage = "Search term is required.")]
        public string Search { get; set; }
        public int PageNumber { get; }
        public int PageSize { get; }

        public GetStampsBySearchQuery(string search, int pageNumber, int pageSize)
        {
            Search = search;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
