using HMS.Core.Entities.SecurityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HMS.Infrastructure.Data.Context
{
    public class AppDbContext : IdentityDbContext<HotelUser>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HotelUser>().ToTable("Users");
            modelBuilder.Entity<StaffUser>().ToTable("StaffUsers");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");


            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
