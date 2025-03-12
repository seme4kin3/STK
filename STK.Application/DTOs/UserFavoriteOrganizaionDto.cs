using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class UserFavoriteOrganizaionDto
    {
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
