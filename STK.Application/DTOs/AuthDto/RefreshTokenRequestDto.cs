
using System.ComponentModel.DataAnnotations;


namespace STK.Application.DTOs.AuthDto
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
