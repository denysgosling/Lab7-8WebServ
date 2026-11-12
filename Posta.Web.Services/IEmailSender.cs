namespace Posta.Web.Services;
public interface IEmailSender { Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default); }
