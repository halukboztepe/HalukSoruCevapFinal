using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortali.Api.DTOs
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
