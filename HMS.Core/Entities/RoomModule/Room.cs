using HMS.Core.Entities.BookingModule;
using HMS.Core.Entities.Enums.RoomEnums;

namespace HMS.Core.Entities.RoomModule
{
    public class Room : BaseEntity<int>
    {
        public string Description { get; set; } = default!;
        public decimal PricePerNight { get; set; }
        public string Amenities { get; set; } = default!;
        public RoomStatus Status { get; set; }
        public RoomType RoomType { get; set; }

        #region Relationships

        #region Images
        public ICollection<RoomImage> Images { get; set; } = [];
        #endregion

        #region Bookings
        public ICollection<BookingEntity> Bookings { get; set; } = [];
        #endregion

        #endregion
    }
}
