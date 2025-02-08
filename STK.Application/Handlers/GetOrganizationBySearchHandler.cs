using MediatR;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Domain.Entities;
using STK.Persistance.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace STK.Application.Handlers
{
    public class GetOrganizationBySearchHandler : IRequestHandler<GetOrganizationBySearchQuery, List<SearchOrganizationDTO>>
    {
        private readonly IOrganizationRepository _repository;

        public GetOrganizationBySearchHandler(IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SearchOrganizationDTO>> Handle(GetOrganizationBySearchQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query.Search))
            {
                throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
            }
            var organization = await _repository.GetOrganizationBySearch(query.Search);

            return organization.Select(o => new SearchOrganizationDTO
            {
                Id = o.Id,
                Name = o.Name,
                FullName = o.FullName,
                Address = o.Adress + o.IndexAdress,
                Inn = o.Requisites.INN,
                Ogrn = o.Requisites.OGRN,
                Managements = o.Managements.Select(m => new SearchManagementDTO
                {
                    FullName = m.FullName,
                    Position = m.Position,
                }).ToList(),
                EconomicActivities = o.EconomicActivities.ToList(),
                
            }).ToList();
            
        }
    }
}
