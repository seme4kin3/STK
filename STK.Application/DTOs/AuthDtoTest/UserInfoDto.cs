using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs.AuthDtoTest
{
    public class UserInfoDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public Guid UserId { get; set; }
        public SubscriptionType Subscription { get; set; }
        public int CountRequest { get; set; }
    }
}
