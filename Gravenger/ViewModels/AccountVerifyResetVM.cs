using System.ComponentModel.DataAnnotations;

namespace Gravenger.ViewModels
{
    public class AccountVerifyResetVM
    {
        [Required]
        [StringLength(maximumLength: 50, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
