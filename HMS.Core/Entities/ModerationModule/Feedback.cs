using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.SecurityModule;

namespace HMS.Core.Entities.ModerationModule
{
    public class Feedback : BaseEntity<Guid>
    {
        public string Comment { get; set; } = default!;
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }

        public string UserId { get; set; } = default!;
        public HotelUser User { get; set; } = default!;

        public Guid BookingId { get; set; }
        public BookingEntity Booking { get; set; } = default!;
    }
}
