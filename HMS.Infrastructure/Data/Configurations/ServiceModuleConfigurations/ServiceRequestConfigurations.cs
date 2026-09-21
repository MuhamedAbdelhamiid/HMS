using HMS.Core.Entities.ServiceModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations.ServiceModuleConfigurations
{
    public class ServiceRequestConfigurations : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId);

            builder.HasOne(s => s.Staff).WithMany().HasForeignKey(s => s.StaffId);

            builder.HasOne(s => s.Admin).WithMany().HasForeignKey(s => s.AdminId);


        }
    }
}
