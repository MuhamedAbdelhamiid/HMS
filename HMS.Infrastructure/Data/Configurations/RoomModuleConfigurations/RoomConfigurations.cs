using HMS.Core.Entities.RoomModuleEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations.RoomModuleConfigurations
{
    public class RoomConfigurations : BaseConfigurations<Room, int>,
        IEntityTypeConfiguration<Room>
    {
        public new void Configure(EntityTypeBuilder<Room> builder)
        {
            base.Configure(builder);

            #region Relationships
            builder.HasMany(r => r.Images)
                .WithOne()
                .HasForeignKey(image => image.RoomId);
            #endregion

            builder.Property(r => r.Id)
                .UseIdentityColumn(100, 1);

            builder.Property(r => r.Description)
                .HasMaxLength(150);

            builder.Property(r => r.PricePerNight)
                .HasPrecision(18, 2);
        }
    }
}
