using System.ComponentModel.DataAnnotations;


namespace STK.Application.DTOs.AuthDto
{
    public class BaseUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
