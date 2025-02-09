using MediatR;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Queries
{
    public class GetOrganizationBySearchQuery : IRequest <PagedList<SearchOrganizationDTO>>
    {
        [Required(ErrorMessage = "Search term is required.")]
        public string Search { get; set; }
        public int PageNumber { get; }
        public int PageSize { get; }


        public GetOrganizationBySearchQuery(string search, int pageNumber, int pageSize)
        {
            Search = search;    
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
