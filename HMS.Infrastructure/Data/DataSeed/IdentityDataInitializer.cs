using HMS.Core.Contracts;
using HMS.Core.Entities.Enums.SecurityEnums;
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

                var staffListToAdd = new List<StaffUser>()
                {
                    new StaffUser()
                    {
                        FullName = "Saeed Mostafa",
                        CreatedAt = DateTime.UtcNow,
                        Email = "saeedmostafa@gmail.com",
                        IsActive = true,
                        UserName = "SaeedMostafa",
                        PhoneNumber = "+2010153912364",
                        Specialities = StaffSpecialities.HouseKeeping
                    },
                    new StaffUser()
                    {
                        FullName = "Mohamed Mohsen",
                        CreatedAt = DateTime.UtcNow,
                        Email = "mohamedmohsen@gmail.com",
                        UserName = "MuMohsen",
                        IsActive = true,
                        PhoneNumber = "+2010153234264",
                        Specialities = StaffSpecialities.Laundry
                    },
                    new StaffUser()
                    {
                        FullName = "Mostafa Mohsen",
                        CreatedAt = DateTime.UtcNow,
                        Email = "mostafamohsen@gmail.com",
                        UserName = "MostafaMohsen",
                        IsActive = true,
                        PhoneNumber = "+201015323264",
                        Specialities = StaffSpecialities.FoodAndBeverage
                    }
                };

                foreach (var staff in staffListToAdd)
                {
                    await _userManager.CreateAsync(staff, "P@ssw0rd");
                    await _userManager.AddToRoleAsync(staff, "Staff");
                }
            }
        }
    }
}
