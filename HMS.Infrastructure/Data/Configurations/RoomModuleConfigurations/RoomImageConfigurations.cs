using HMS.Core.Entities.RoomModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations.RoomModuleConfigurations
{
    public class RoomImageConfigurations : BaseConfigurations<RoomImage, int>, IEntityTypeConfiguration<RoomImage>
    {
        public new void Configure(EntityTypeBuilder<RoomImage> builder)
        {
            base.Configure(builder);

            builder.Property(ri => ri.ImageUrl)
                .HasMaxLength(150)
                .IsRequired();
        }
    }
}
