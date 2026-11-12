using FluentValidation;
using Posta.Web.Validators;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Posta.Web.Services;
using Posta.Web.Models;
using Posta.Web.Logging;
namespace Posta.Web;
public sealed class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddControllersWithViews(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, _) =>
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "uk"
                    ? "Введіть значення правильного типу." : "Enter a value of the correct type.");
        }).AddViewLocalization().AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource));
        });
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = new[] { new CultureInfo("uk-UA"), new CultureInfo("en-US") };
            options.DefaultRequestCulture = new RequestCulture("uk-UA");
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
            options.RequestCultureProviders = [new CookieRequestCultureProvider()];
            options.ApplyCurrentCultureToResponseHeaders = true;
        });
        services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(environment.ContentRootPath, "App_Data", "Keys")));
        services.AddAntiforgery(options => options.Cookie.Name = "Posta.Antiforgery.Lab7-8");
        services.Configure<EmailOptions>(configuration.GetSection("EmailSender"));
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddScoped<IValidator<RequestForm>, RequestFormValidator>();
        services.AddSingleton(TimeProvider.System);
        services.AddTransient<TransientProbe>();
        services.AddScoped<ScopedProbe>();
        services.AddSingleton<SingletonProbe>();
        services.AddLogging(logging => {
            logging.AddProvider(new FileLoggerProvider(Path.Combine(environment.ContentRootPath, "Logs")));
        });
    }
    public void Configure(WebApplication app)
    {
        app.UseRequestLocalization();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseExceptionHandler("/Home/Error");
        app.Use(async (context, next) => {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            await next();
        });
        app.UseWhen(c => !c.Request.Path.StartsWithSegments("/Files"), branch => branch.UseStaticFiles());
        app.UseRouting();
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    }
}
