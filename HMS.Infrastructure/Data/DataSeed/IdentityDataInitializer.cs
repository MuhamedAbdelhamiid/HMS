using HMS.Core.Contracts;
using HMS.Core.Entities.SecurityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HMS.Infrastructure.Data.DataSeed
{
    public class IdentityDataInitializer : IDataInitializer
    {
        private readonly UserManager<HotelUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityDataInitializer(
            UserManager<HotelUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task InitializeAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var users = await _userManager.Users.ToListAsync();

            if (roles is null || !roles.Any())
            {
                var rolesToCreate = new List<IdentityRole>()
                {
                    new IdentityRole() {Name = "Admin"},
                    new IdentityRole() {Name = "Staff"},
                    new IdentityRole() {Name = "Guest"}
                };

                foreach (var role in rolesToCreate)
                {
                    await _roleManager.CreateAsync(role);
                }
            }

            if (users is null || !users.Any())
            {
                var adminToAdd = new HotelUser()
                {
                    FullName = "Mohamed Abdelhamid",
                    CreatedAt = DateTime.UtcNow,
                    Email = "muhamedabdelhamiid@gmail.com",
                    IsActive = true,
                    PhoneNumber = "+201015390264",
                    UserName = "MuAbdelhamiid"
                };

                await _userManager.CreateAsync(adminToAdd, "P@ssw0rd");
                await _userManager.AddToRoleAsync(adminToAdd, "Admin");
            }
        }
    }
}
