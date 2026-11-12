using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc;
using Posta.Web.Models;
using Posta.Web.Services;
namespace Posta.Web.Controllers;
public sealed class MailController(IEmailSender sender, ILogger<MailController> logger, IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet] public IActionResult Index() => View(new MailForm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MailForm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try {
            await sender.SendAsync(model.Recipient, model.Subject, model.Message, cancellationToken);
            TempData["Success"] = localizer["Text098"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception) {
            logger.LogError(exception, "SMTP delivery failed");
            ModelState.AddModelError("", localizer["Text099"].Value);
            return View(model);
        }
    }
}
