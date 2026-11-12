using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Posta.Web.Models;

namespace Posta.Web.Controllers;

public sealed class RequestsController(
    IValidator<RequestForm> validator,
    IStringLocalizer<SharedResource> text) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new RequestForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RequestForm model, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(model, cancellationToken);
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

        if (!ModelState.IsValid)
            return View(model);

        TempData["RequestSuccess"] = text["RequestSuccess"].Value;
        return RedirectToAction(nameof(Index));
    }
}
