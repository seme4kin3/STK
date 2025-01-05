using MediatR;
using STK.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Queries
{
    public class GetOrganizationsQuery : IRequest<List<OrganizationDto>>
    {
    }
}
