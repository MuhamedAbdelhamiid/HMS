using HMS.Shared.Messages;

namespace HMS.Services.Abstraction
{
    public interface IEmailService
    {
        Task SendEmailAsync(Email email);
    }
}
