namespace Posta.Web.Services;
public sealed class EmailOptions
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Security { get; set; } = "StartTls";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "Posta";
    public int TimeoutMilliseconds { get; set; } = 15000;
}
