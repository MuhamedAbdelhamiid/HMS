using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.RoomModuleDTOs
{
    public class AdminRoomCreationDTO
    {
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = default!;
        [Required(ErrorMessage = "Price per night is required")]
        [Range(0, double.MaxValue, ErrorMessage = "price must be positive.")]
        public decimal PricePerNight { get; set; }
        [Required(ErrorMessage = "RoomType is required")]
        public string RoomType { get; set; } = default!;
        [Required(ErrorMessage = "Amenities is required")]
        public string Amenities { get; set; } = default!;
    }
}
