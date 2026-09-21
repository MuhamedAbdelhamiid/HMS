using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.BookingEnums;

namespace HMS.Services.Helpers
{
    public static class BookingServiceHelper
    {
        public static bool BookingIsValid(BookingEntity booking)
        => booking.Status == BookingStatus.Paid && booking.CheckInDate >= DateTime.UtcNow && booking.CheckOutDate > DateTime.UtcNow;
    }
}
