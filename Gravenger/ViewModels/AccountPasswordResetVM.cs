using System.ComponentModel.DataAnnotations;

namespace Gravenger.ViewModels
{
    public class AccountPasswordResetVM
    {
        [Required]
        [Display(Name = "Username or Email address")]
        public string UsernameOrEmail { get; set; }
    }
}
