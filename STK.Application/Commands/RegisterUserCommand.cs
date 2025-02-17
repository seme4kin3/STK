using MediatR;
using STK.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Commands
{
    public class RegisterUserCommand : IRequest<bool>
    {
        public RegisterDto Register { get; set; }
    }
}

