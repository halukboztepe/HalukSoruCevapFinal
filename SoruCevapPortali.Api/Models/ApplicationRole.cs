using Microsoft.AspNetCore.Identity;

namespace SoruCevapPortali.Api.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
            UserRoles = new HashSet<ApplicationUserRole>();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            UserRoles = new HashSet<ApplicationUserRole>();
        }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
} 