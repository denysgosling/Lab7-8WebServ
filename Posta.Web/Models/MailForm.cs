using System.ComponentModel.DataAnnotations;
namespace Posta.Web.Models;
public sealed class MailForm
{
    [Required(ErrorMessage="Text092"), EmailAddress(ErrorMessage="Text093"), StringLength(254, ErrorMessage="RecipientTooLong")]
    [Display(Name="Text034")]
    public string Recipient { get; set; } = "";
    [Required(ErrorMessage="Text094"), StringLength(150, ErrorMessage="Text095")]
    [Display(Name="Text035")]
    public string Subject { get; set; } = "";
    [Required(ErrorMessage="Text096"), StringLength(5000, ErrorMessage="Text097")]
    [Display(Name="Text002")]
    public string Message { get; set; } = "";
}
