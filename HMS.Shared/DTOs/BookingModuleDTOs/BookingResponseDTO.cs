namespace HMS.Shared.DTOs.BookingModuleDTOs
{
    public class BookingResponseDTO
    {
        public Guid Id { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = default!;
    }
}
