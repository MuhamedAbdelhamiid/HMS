using HMS.Core.Entities.RoomModuleEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
