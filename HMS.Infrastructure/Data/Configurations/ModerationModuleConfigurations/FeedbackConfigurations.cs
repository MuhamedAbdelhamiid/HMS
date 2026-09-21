using HMS.Core.Entities.ModerationModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Data.Configurations.ModerationModuleConfigurations
{
    public class FeedbackConfigurations : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.HasOne(feed => feed.User)
                .WithMany()
                .HasForeignKey(feed => feed.UserId);

            builder.HasOne(feed => feed.Booking)
                .WithMany()
                .HasForeignKey(feed => feed.BookingId);
        }
    }
}
