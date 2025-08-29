using MediatR;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using System.ComponentModel.DataAnnotations;

namespace STK.Application.Queries
{
    public class GetFavoriteOrganizationBySearchQuery : IRequest<PagedList<SearchOrganizationDTO>>
    {
        [Required(ErrorMessage = "Search term is required.")]
        public string Search { get; set; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public Guid UserId { get; set; }

        public GetFavoriteOrganizationBySearchQuery(string search, int pageNumber, int pageSize, Guid userId)
        {
            Search = search;
            PageNumber = pageNumber;
            PageSize = pageSize;
            UserId = userId;
        }
    }
}
