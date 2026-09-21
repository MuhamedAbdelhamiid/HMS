namespace HMS.Shared.DTOs.BookingModuleDTOs
{
    public class BookingDTO
    {
        public Guid Id { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = default!;
        public string UserFullName { get; set; } = default!;
        public string UserId { get; set; } = default!;
    }
}
