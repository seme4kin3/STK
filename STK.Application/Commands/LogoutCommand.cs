using MediatR;
using STK.Application.DTOs.SearchOrganizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Commands
{
    public class LogoutCommand : IRequest<AuthUserResponse>
    {
        public Guid UserId { get; set; }
    }
}
