using MediatR;
using STK.Application.DTOs;
using STK.Application.Pagination;

namespace STK.Application.Queries
{
    public class GetTenderBySearchQuery : IRequest<PagedList<TenderDto>>
    {
        public string? Search { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public GetTenderBySearchQuery(string? search, int pageNumber, int pageSize, DateTime? staretedDate, DateTime? endDate) 
        { 
            Search = search;
            StartedDate = staretedDate;
            EndDate = endDate;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
