using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc;
using Posta.Web.Models;
namespace Posta.Web.Controllers;
public sealed class FilesController(IWebHostEnvironment environment, IStringLocalizer<SharedResource> localizer) : Controller
{
    private const long Limit = 20 * 1024 * 1024;
    private static readonly HashSet<string> Allowed = [".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf", ".txt", ".csv", ".zip", ".docx", ".xlsx", ".pptx"];
    private string Folder { get { var path = Path.Combine(environment.WebRootPath, "Files"); Directory.CreateDirectory(path); return path; } }
    private static StoredFile Describe(FileInfo file) {
        var display = file.Name.Length > 33 && Guid.TryParseExact(file.Name[..32], "N", out _) && file.Name[32] == '_' ? file.Name[33..] : file.Name;
        return new(file.Name, display, file.Length, file.CreationTimeUtc, file.Extension.ToLowerInvariant());
    }
    private string? Find(string? name) {
        if (string.IsNullOrWhiteSpace(name) || name.StartsWith('.') || name.Contains('/') || name.Contains('\\') || name != Path.GetFileName(name)) return null;
        var path = Path.Combine(Folder, name);
        return System.IO.File.Exists(path) ? path : null;
    }
    [HttpGet] public IActionResult Index() => View(Directory.EnumerateFiles(Folder).Where(p => !Path.GetFileName(p).StartsWith('.')).Select(p => Describe(new FileInfo(p))).OrderByDescending(f => f.Uploaded).ToArray());
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(Limit + 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken) {
        if (file is null || file.Length == 0) { TempData["Error"] = localizer["Text100"].Value; return RedirectToAction(nameof(Index)); }
        if (file.Length > Limit) { TempData["Error"] = localizer["Text101"].Value; return RedirectToAction(nameof(Index)); }
        var name = Path.GetFileName(file.FileName.Replace('\\', '/'));
        var extension = Path.GetExtension(name).ToLowerInvariant();
        if (name.Length > 120 || name.Any(c => char.IsControl(c) || Path.GetInvalidFileNameChars().Contains(c)) || !Allowed.Contains(extension)) {
            TempData["Error"] = localizer["Text102"].Value;
            return RedirectToAction(nameof(Index));
        }
        var destination = Path.Combine(Folder, $"{Guid.NewGuid():N}_{name}");
        try {
            await using var stream = new FileStream(destination, FileMode.CreateNew);
            await file.CopyToAsync(stream, cancellationToken);
        } catch { if (System.IO.File.Exists(destination)) System.IO.File.Delete(destination); throw; }
        TempData["Success"] = localizer["Text118", name].Value;
        return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Details(string? name) {
        var path = Find(name); if (path is null) return NotFound();
        var file = Describe(new FileInfo(path));
        string? text = null;
        if (file.IsText) { using var reader = new StreamReader(path); var buffer = new char[100_001]; var count = await reader.ReadBlockAsync(buffer); text = new string(buffer, 0, Math.Min(count, 100_000)); if (count > 100_000) text += "\n… " + localizer["Text120"].Value; }
        return View(new FileDetails(file, text));
    }
    [HttpGet] public IActionResult Download(string? name) {
        var path = Find(name); return path is null ? NotFound() : PhysicalFile(path, "application/octet-stream", Describe(new FileInfo(path)).DisplayName);
    }
    [HttpGet] public IActionResult Preview(string? name) {
        var path = Find(name); if (path is null) return NotFound();
        var type = Path.GetExtension(path).ToLowerInvariant() switch { ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".gif" => "image/gif", ".webp" => "image/webp", ".pdf" => "application/pdf", _ => null };
        if (type is null) return NotFound();
        Response.Headers["Content-Security-Policy"] = "sandbox";
        Response.Headers.ContentDisposition = "inline";
        return PhysicalFile(path, type, enableRangeProcessing: true);
    }
}
