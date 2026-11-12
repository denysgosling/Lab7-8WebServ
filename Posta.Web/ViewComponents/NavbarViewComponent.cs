using Microsoft.AspNetCore.Mvc;
using Posta.Web.Models;
namespace Posta.Web.ViewComponents;
public sealed class NavbarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var controller = ViewContext.RouteData.Values["controller"]?.ToString() ?? "Home";
        var title = ViewContext.ViewData["Title"]?.ToString() ?? "Posta";
        return View(new NavigationModel(controller, title));
    }
}
