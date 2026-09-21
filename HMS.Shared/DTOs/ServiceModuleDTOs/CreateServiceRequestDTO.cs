using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.ServiceModuleDTOs
{
    public class CreateServiceRequestDTO
    {
        [Required(ErrorMessage = "Service id is required.")]
        public int ServiceId { get; set; }
        [Required(ErrorMessage = "Booking id is required.")]
        public Guid BookingId { get; set; }
        [Required(ErrorMessage = "Room number is required.")]
        public int RoomNumber { get; set; }
        public string? Notes { get; set; }
    }
}
