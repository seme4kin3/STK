using MediatR;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Domain.Entities;
using STK.Persistance.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Handlers
{
    public class GetOrganizationsQueryHandler: IRequestHandler<GetOrganizationsQuery, List<SearchOrganizationDTO>>
    {
        private readonly IOrganizationRepository _repository;

        public GetOrganizationsQueryHandler(IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SearchOrganizationDTO>> Handle (GetOrganizationsQuery query, CancellationToken cancellationToken)
        {
            var organizations = await _repository.GetAllOrganizations();

            return organizations.Select
                (o => new SearchOrganizationDTO
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
                    EconomicActivities = o.EconomicActivities
                        
                        .Select(e => new EconomicActivity
                        {
                            OKVDNnumber = e.OKVDNnumber

                        }).ToList(),
                            

                }).ToList();
        }
    }
}
