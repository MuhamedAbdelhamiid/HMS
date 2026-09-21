using HMS.Infrastructure.ExternalServices.ServiceHelperEntites;
using HMS.Services.Abstraction;
using HMS.Shared.Messages;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace HMS.Infrastructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<EmailSettings> _options;

        public EmailService(IOptions<EmailSettings> options)
        {
            _options = options;
        }
        public async Task SendEmailAsync(Email email)
        {
            var mail = new MimeMessage()
            {
                Sender = MailboxAddress.Parse(_options.Value.Email),
                Subject = email.Subject
            };
            mail.To.Add(InternetAddress.Parse(email.To));
            mail.From.Add(new MailboxAddress(_options.Value.DisplayName, _options.Value.Email));

            var builder = new BodyBuilder();
            builder.TextBody = email.Message;

            mail.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_options.Value.Host, _options.Value.Port, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_options.Value.Email, _options.Value.Password);

            await smtp.SendAsync(mail);

            await smtp.DisconnectAsync(true);
        }
    }
}
