namespace HMS.Shared.DTOs.RoomModuleDTOs
{
    public class RoomDetailsDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = default!;
        public decimal PricePerNight { get; set; }
        public string Amenities { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string RoomType { get; set; } = default!;
        public ICollection<RoomImageDTO> Images { get; set; } = [];
    }
}
