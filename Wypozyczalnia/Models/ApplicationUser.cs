using Microsoft.AspNetCore.Identity;

namespace Wypozyczalnia.Models
{
    public class ApplicationUser : IdentityUser
    {
        // FullName może być opcjonalne; jeśli chcesz wymusić, ustaw = string.Empty
        public string? FullName { get; set; }
    }
}
