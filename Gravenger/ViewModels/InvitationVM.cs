using System.ComponentModel.DataAnnotations;

namespace Gravenger.ViewModels
{
    public class InvitationVM
    {
        [Required]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }
}
