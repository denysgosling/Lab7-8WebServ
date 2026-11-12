namespace Posta.Web.Models;

public sealed class RequestForm
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? Age { get; set; }
    public string? Subject { get; set; }
    public string? Category { get; set; }
    public string? Message { get; set; }
    public string? Website { get; set; }
    public DateOnly? PreferredDate { get; set; }
}
