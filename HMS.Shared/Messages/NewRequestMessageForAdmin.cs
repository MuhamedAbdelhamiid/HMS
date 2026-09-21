namespace HMS.Shared.Messages
{
    public class NewRequestMessageForAdmin
    {
        public Guid RequestId { get; set; }
        public string ServiceName { get; set; } = default!;
        public string GuestName { get; set; } = default!;
        public int RoomNumber { get; set; } = default!;
        public string? Notes { get; set; } = default!;
    }
}
