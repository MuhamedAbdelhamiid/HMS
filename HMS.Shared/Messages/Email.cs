namespace HMS.Shared.Messages
{
    public class Email
    {
        public string To { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}
