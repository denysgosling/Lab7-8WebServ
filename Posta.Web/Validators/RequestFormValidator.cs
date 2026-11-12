using FluentValidation;
using Microsoft.Extensions.Localization;
using Posta.Web.Models;

namespace Posta.Web.Validators;

public sealed class RequestFormValidator : AbstractValidator<RequestForm>
{
    public RequestFormValidator(IStringLocalizer<SharedResource> text, TimeProvider clock)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(text["RequestFirstNameRequired"])
            .Length(2, 40).WithMessage(text["RequestNameLength"])
            .Matches(@"^[\p{L}][\p{L}\p{M}'’ -]*$").WithMessage(text["RequestNameFormat"]);
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(text["RequestLastNameRequired"])
            .Length(2, 40).WithMessage(text["RequestNameLength"])
            .Matches(@"^[\p{L}][\p{L}\p{M}'’ -]*$").WithMessage(text["RequestNameFormat"]);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(text["RequestEmailRequired"])
            .MaximumLength(254).WithMessage(text["RequestEmailLength"])
            .EmailAddress().WithMessage(text["RequestEmailFormat"]);
        RuleFor(x => x.Phone)
            .Matches(@"^\+380\d{9}$").WithMessage(text["RequestPhoneFormat"])
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.Age)
            .NotNull().WithMessage(text["RequestAgeRequired"])
            .InclusiveBetween(16, 100).WithMessage(text["RequestAgeRange"]);
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage(text["RequestSubjectRequired"])
            .Length(5, 100).WithMessage(text["RequestSubjectLength"]);
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage(text["RequestCategoryRequired"])
            .Must(value => value is "Design" or "Development" or "Support")
            .WithMessage(text["RequestCategoryInvalid"]);
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(text["RequestMessageRequired"])
            .Length(20, 2000).WithMessage(text["RequestMessageLength"]);
        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage(text["RequestWebsiteLength"])
            .Must(value => Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                && !string.IsNullOrWhiteSpace(uri.Host))
            .WithMessage(text["RequestWebsiteFormat"])
            .When(x => !string.IsNullOrWhiteSpace(x.Website));
        RuleFor(x => x.PreferredDate)
            .Must(value =>
            {
                var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
                return value >= today && value <= today.AddDays(90);
            })
            .WithMessage(text["RequestDateRange"])
            .When(x => x.PreferredDate.HasValue);
    }
}
