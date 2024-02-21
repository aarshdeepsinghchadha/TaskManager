using Microsoft.AspNetCore.Identity;

namespace Domain
{
    /// <summary>
    /// AppUser represents a user in the system, extending IdentityUser.
    /// </summary>
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
