using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
public class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    public EmailSender(IOptions<EmailSettings> options) => _settings = options.Value;

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}