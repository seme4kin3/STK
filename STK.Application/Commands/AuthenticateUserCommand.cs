using MediatR;
using STK.Application.DTOs.AuthDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Commands
{
    public class AuthenticateUserCommand : IRequest<AuthTokenResponse>
    {
        public UserDto AuthDto { get; set; }
    }
}
