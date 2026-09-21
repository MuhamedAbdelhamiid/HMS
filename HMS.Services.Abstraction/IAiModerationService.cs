namespace HMS.Services.Abstraction
{
    public interface IAiModerationService
    {
        Task<bool> IsUserCommentAcceptedAsFeedbackAsync(string comment);
    }
}
