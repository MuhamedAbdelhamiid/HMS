namespace HMS.Shared.Messages
{
    public class StatusUpdateForUser
    {
        public Guid RequestId { get; set; }
        public string NewStatus { get; set; } = default!;
    }
}
