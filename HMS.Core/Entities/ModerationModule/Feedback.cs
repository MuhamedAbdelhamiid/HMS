using HMS.Core.Entities.BookingModule;

namespace HMS.Core.Entities.ModerationModule
{
    public class Feedback : BaseEntity<Guid>
    {
        public string Comment { get; set; } = default!;
        public bool IsApproved { get; set; }

        public Guid BookingId { get; set; }
        public BookingEntity Booking { get; set; } = default!;
    }
}
