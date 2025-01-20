using MediatR;
using STK.Application.DTOs;
using STK.Application.DTOs.ListOrganizations;
using STK.Application.Queries;
using STK.Persistance.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Handlers
{
    public class GetOrganizationsQueryHandler: IRequestHandler<GetOrganizationsQuery, List<ConciseOrganizationsDto>>
    {
        private readonly IOrganizationRepository _repository;

        public GetOrganizationsQueryHandler(IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ConciseOrganizationsDto>> Handle (GetOrganizationsQuery query, CancellationToken cancellationToken)
        {
            var organizations = await _repository.GetAllOrganizations();

            return organizations
                .Select(o => new ConciseOrganizationsDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    FullName = o.FullName,
                    Adress = $"{o.Adress}, {o.IndexAdress}",
                    EconomicActivities = o.EconomicActivities.Select(e => new EconomicActivityDto
                    {
                        OKVDNnumber = e.OKVDNnumber,
                        Discription = e.Discription,
                    }).ToList(),
                    ConciseRequisite = new ConciseRequisiteDto
                    {
                        INN = o.Requisites.INN,
                        KPP = o.Requisites.KPP,
                        AuthorizedCapital = o.Requisites.AuthorizedCapital
                    }
                }).ToList();
        }
    }
}
