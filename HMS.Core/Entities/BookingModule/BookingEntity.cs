using HMS.Core.Entities.Enums.BookingEnums;
using HMS.Core.Entities.RoomModule;
using HMS.Core.Entities.SecurityModule;

namespace HMS.Core.Entities.BookingModule
{
    public class BookingEntity : BaseEntity<Guid>
    {
        public BookingStatus Status { get; set; } = default!;
        public DateTime CheckInDate { get; set; } = default!;
        public DateTime CheckOutDate { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string? PaymobOrderId { get; set; }
        public string? PaymobPaymentKey { get; set; }
        public DateTime? PaidDate { get; set; }

        #region Relationships

        #region Room
        public Room Room { get; set; } = default!;
        public int RoomId { get; set; }
        #endregion

        #region HotelUser
        public HotelUser User { get; set; } = default!;
        public string HotelUserId { get; set; } = default!;
        #endregion

        #endregion
    }
}
