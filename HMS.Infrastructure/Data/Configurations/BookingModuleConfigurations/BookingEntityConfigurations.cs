using HMS.Core.Entities.BookingModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations.BookingModuleConfigurations
{
    internal class BookingEntityConfigurations : IEntityTypeConfiguration<BookingEntity>
    {
        public void Configure(EntityTypeBuilder<BookingEntity> builder)
        {
            builder.Property(b => b.TotalAmount).HasPrecision(8, 2);
        }
    }
}
