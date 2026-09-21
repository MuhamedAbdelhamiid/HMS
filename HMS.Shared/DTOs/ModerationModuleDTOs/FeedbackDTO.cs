namespace HMS.Shared.DTOs.ModerationModuleDTOs
{
    public class FeedbackDTO
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; } = default!;
        public string Comment { get; set; } = default!;
    }
}
