using HMS.Core.Entities.Enums.RoomEnums;

namespace HMS.Core.Entities.RoomModuleEntities
{
    public class Room : BaseEntity<int>
    {
        public string Description { get; set; } = default!;
        public decimal PricePerNight { get; set; }
        public string Amenities { get; set; } = default!;
        public RoomStatus Status { get; set; }
        public RoomType RoomType { get; set; }
        public ICollection<RoomImage> Images { get; set; } = [];
    }
}
