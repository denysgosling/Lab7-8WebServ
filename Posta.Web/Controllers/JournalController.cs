using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Posta.Web.Models;
namespace Posta.Web.Controllers;
public sealed class JournalController(IWebHostEnvironment environment) : Controller
{
    public IActionResult Index(string? q) {
        var folder = Path.Combine(environment.ContentRootPath, "Logs");
        var latest = Directory.Exists(folder) ? Directory.GetFiles(folder, "*.log").OrderDescending().FirstOrDefault() : null;
        string[] entries = [];
        if (latest is not null) {
            using var stream = new FileStream(latest, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var tail = new Queue<string>();
            while (reader.ReadLine() is { } line) {
                if (!line.Contains("URL:")) continue;
                tail.Enqueue(line);
                if (tail.Count > 20) tail.Dequeue();
            }
            entries = tail.Reverse().ToArray();
        }
        return View(new JournalPage(q ?? "", Request.GetDisplayUrl(), entries));
    }
}
