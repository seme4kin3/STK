using MediatR;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Queries
{
    public class GetOrganizationBySearchQuery : IRequest <List<SearchOrganizationDTO>>
    {
        [Required(ErrorMessage = "Search term is required.")]
        public string Search { get; set; }
        

        public GetOrganizationBySearchQuery(string search)
        {
            Search = search;
        }
    }
}
