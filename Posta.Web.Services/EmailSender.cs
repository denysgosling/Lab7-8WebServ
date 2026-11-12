using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
namespace Posta.Web.Services;
public sealed class EmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.Host) || !MailboxAddress.TryParse(config.FromEmail, out var sender))
            throw new InvalidOperationException("Налаштуйте EmailSender:Host та EmailSender:FromEmail.");
        var security = config.Security switch {
            "StartTls" => SecureSocketOptions.StartTls,
            "SslOnConnect" => SecureSocketOptions.SslOnConnect,
            "None" when config.Host is "localhost" or "127.0.0.1" => SecureSocketOptions.None,
            _ => throw new InvalidOperationException("Невідомий або небезпечний режим SMTP.")
        };
        var mail = new MimeMessage();
        mail.From.Add(new MailboxAddress(config.FromName, sender.Address));
        mail.To.Add(MailboxAddress.Parse(recipient));
        mail.Subject = subject;
        mail.Body = new TextPart("plain") { Text = message };
        using var client = new SmtpClient { Timeout = config.TimeoutMilliseconds };
        await client.ConnectAsync(config.Host, config.Port, security, cancellationToken);
        try {
            if (!string.IsNullOrWhiteSpace(config.UserName))
                await client.AuthenticateAsync(config.UserName, config.Password, cancellationToken);
            await client.SendAsync(mail, cancellationToken);
        }
        finally {
            if (client.IsConnected) await client.DisconnectAsync(true, CancellationToken.None);
        }
    }
}
