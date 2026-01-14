using Microsoft.AspNetCore.Identity;

namespace Wypozyczalnia.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
