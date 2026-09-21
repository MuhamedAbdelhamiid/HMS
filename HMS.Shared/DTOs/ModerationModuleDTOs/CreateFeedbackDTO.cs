using System.ComponentModel.DataAnnotations;

namespace HMS.Shared.DTOs.ModerationModuleDTOs
{
    public class CreateFeedbackDTO
    {
        [Required(ErrorMessage = "Booking id is required.")]
        public Guid BookingId { get; set; }
        [Required(ErrorMessage = "Comment is required.")]
        public string Comment { get; set; } = default!;
    }
}
