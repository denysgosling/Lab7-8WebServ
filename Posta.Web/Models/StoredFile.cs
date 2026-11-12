namespace Posta.Web.Models;
public record StoredFile(string Name, string DisplayName, long Length, DateTimeOffset Uploaded, string Extension)
{
    public string Size => Length < 1024 * 1024 ? $"{Length / 1024.0:F1} KB" : $"{Length / 1024.0 / 1024:F1} MB";
    public bool IsImage => Extension is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp";
    public bool IsText => Extension is ".txt" or ".csv";
}
public record FileDetails(StoredFile File, string? Text);
