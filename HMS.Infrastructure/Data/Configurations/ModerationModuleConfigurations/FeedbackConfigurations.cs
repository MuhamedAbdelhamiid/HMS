using HMS.Core.Entities.ModerationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations.ModerationModuleConfigurations
{
    public class FeedbackConfigurations : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder
                .HasOne(f => f.Booking)
                .WithMany()
                .HasForeignKey(f => f.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
