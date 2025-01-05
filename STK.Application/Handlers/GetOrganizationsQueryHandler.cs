using MediatR;
using STK.Application.DTOs;
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
    public class GetOrganizationsQueryHandler: IRequestHandler<GetOrganizationsQuery, List<OrganizationDto>>
    {
        private readonly IOrganizationRepository _repository;

        public GetOrganizationsQueryHandler(IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OrganizationDto>> Handle (GetOrganizationsQuery query, CancellationToken cancellationToken)
        {
            var organizations = await _repository.GetAllOrganizations();

            return organizations
                .Select(o => new OrganizationDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    FullName = o.FullName,
                    Adress = $"{o.Adress} {o.IndexAdress}",
                    EconomicActivities = o.EconomicActivities.Select(e => new EconomicActivityDto
                    {
                        OKVDNnumber = e.OKVDNnumber,
                        Discription = e.Discription,
                    }).ToList(),
                    Requisites = new RequisiteDto
                    {
                        INN = o.Requisites.INN,
                        KPP = o.Requisites.KPP,
                        AuthorizedCapital = o.Requisites.AuthorizedCapital
                    }
                    //(r => new RequisiteDto
                    //{
                    //    INN = r.INN,
                    //    KPP = r.KPP,
                    //    OGRN = r.OGRN,
                    //    AuthorizedCapital = r.AuthorizedCapital
                    //}).ToList(),

                }).ToList();
        }
    }
}
