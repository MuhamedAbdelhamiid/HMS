namespace HMS.Shared.DTOs.SecurityModuleDTOs
{
    public class BookingDTO
    {
        public Guid BookingId { get; set; } = default!;
        public string RoomId { get; set; } = default!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = default!;
    }
}