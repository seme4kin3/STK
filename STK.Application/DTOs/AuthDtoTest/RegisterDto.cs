using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs.AuthDtoTest
{
    public class RegisterDto : BaseUserDto
    {
        public RoleName Role { get; set; }
        public SubscriptionType Subscription { get; set; }
    }
}
