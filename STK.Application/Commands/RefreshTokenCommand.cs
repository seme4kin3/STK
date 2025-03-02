using MediatR;
using STK.Application.DTOs.AuthDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Commands
{
    public class RefreshTokenCommand: IRequest<AuthTokenResponse>
    {
        public RefreshTokenDto RefreshTokenRequest { get; set; }
    }
}
