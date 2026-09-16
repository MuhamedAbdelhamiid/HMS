using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.BookingModuleDTOs
{
    public class CreateBookingDTO
    {
        [Required(ErrorMessage = "RoomId is required.")]
        public int RoomId { get; set; }
        [Required(ErrorMessage = "CheckIn date is required.")]
        public DateTime CheckInDate { get; set; }
        [Required(ErrorMessage = "CheckOut date is required.")]
        public DateTime CheckOutDate { get; set; }
    }
}
