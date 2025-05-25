using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortali.Api.DTOs
{
    public class AssignRoleDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }
    }
}