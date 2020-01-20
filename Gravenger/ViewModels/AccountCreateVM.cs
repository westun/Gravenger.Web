using System.ComponentModel.DataAnnotations;

namespace Gravenger.ViewModels
{
    public class AccountCreateVM
    {
        [Required]
        [Display(Name = "Name")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "Please enter a valid Name.")]
        public string Name { get; set; }
        
        [Required]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 4, ErrorMessage = "Please enter a valid Username.")]
        [RegularExpression(@"^\S*$", ErrorMessage = "Username cannot contain spaces.")]
        public string Username { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-z])(?=.*[A-Z])(?=.*[0-9])\S{8,50}$", ErrorMessage = "Password requires at least one lower and one upper case letter, a number, no spaces, and must be 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
