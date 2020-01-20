using System;
using System.ComponentModel.DataAnnotations;

namespace Gravenger.Areas.Admin.ViewModels
{
    public class EmailVerificationVM
    {
        public int? AccountID { get; set; }
        public string Email { get; set; }
        public string Token { get; private set; }
        public bool Used { get; set; }
        public DateTimeOffset Expires { get; set; }
        public DateTimeOffset CreatedDate { get; private set; }
        public int? CreatedByAccountID { get; set; }

        public bool IsExpired
        {
            get
            {
                return DateTimeOffset.UtcNow > this.Expires;
            }
        }

        public bool IsValid
        {
            get
            {
                return !this.IsExpired && !this.Used;
            }
        }
    }
}