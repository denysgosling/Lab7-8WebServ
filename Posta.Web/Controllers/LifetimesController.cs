using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc;
using Posta.Web.Models;
namespace Posta.Web.Controllers;
public sealed class LifetimesController(IStringLocalizer<SharedResource> localizer) : Controller
{
    public IActionResult Index() {
        var services = HttpContext.RequestServices;
        return View(new LifetimeRow[] {
            new("Transient", localizer["Text055"].Value, services.GetRequiredService<TransientProbe>().Id, services.GetRequiredService<TransientProbe>().Id),
            new("Scoped", localizer["Text056"].Value, services.GetRequiredService<ScopedProbe>().Id, services.GetRequiredService<ScopedProbe>().Id),
            new("Singleton", localizer["Text057"].Value, services.GetRequiredService<SingletonProbe>().Id, services.GetRequiredService<SingletonProbe>().Id)
        });
    }
}
