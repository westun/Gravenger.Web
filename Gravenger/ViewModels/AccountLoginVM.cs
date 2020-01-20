using System.ComponentModel.DataAnnotations;

namespace Gravenger.ViewModels
{
    public class AccountLoginVM
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
        
    }
}
