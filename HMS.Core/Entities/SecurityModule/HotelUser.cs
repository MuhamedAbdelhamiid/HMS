using Microsoft.AspNetCore.Identity;

namespace HMS.Core.Entities.SecurityModule
{
    public class HotelUser : IdentityUser
    {
        public bool IsActive { get; set; }
        public string FullName { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
