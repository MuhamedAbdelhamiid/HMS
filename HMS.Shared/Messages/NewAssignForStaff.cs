namespace HMS.Shared.Messages
{
    public class NewAssignForStaff
    {
        public Guid RequestId { get; set; }
        public string ServiceName { get; set; } = default!;
        public int RoomNumber { get; set; }
    }
}
