using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.RoomModuleDTOs
{
    public class AdminRoomUpdateDTO
    {
        [Required(ErrorMessage = "Room type is required.")]
        public string RoomType { get; set; } = default!;
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = default!;
        [Required(ErrorMessage = "Price per night is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price per night must be a positive number.")]
        public decimal PricePerNight { get; set; }
        [Required(ErrorMessage = "Amenities is required.")]
        public string Amenities { get; set; } = default!;
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = default!;
    }
}
