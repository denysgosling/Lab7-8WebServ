using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Posta.Web.Models;
namespace Posta.Web.Controllers;
public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        if (culture is not ("uk-UA" or "en-US")) return BadRequest();
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = true, Secure = Request.IsHttps, IsEssential = true, SameSite = SameSiteMode.Lax });
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    public IActionResult Error() { Response.StatusCode = 500; return View(new ErrorInfo(HttpContext.TraceIdentifier)); }
}
