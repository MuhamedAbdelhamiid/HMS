namespace HMS.Shared.DTOs.RoomModuleDTOs
{
    public class AdminRoomDTO
    {
        public int Id { get; set; }
        public decimal PricePerNight { get; set; }
        public string Amenities { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string RoomType { get; set; } = default!;
        public string CreatedAt { get; set; } = default!;
        public string UpdatedAt { get; set; } = default!;
    }
}
