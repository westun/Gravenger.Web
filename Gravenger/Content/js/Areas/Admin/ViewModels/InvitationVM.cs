using Gravenger.Domain.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Gravenger.Areas.Admin.ViewModels
{
    public class InvitationVM
    {
        public int InvitationID { get; set; }
        public int? AccountID { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public bool Accepted { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public int CreatedByAccountID { get; set; }
        public Account CreatedByAccount { get; set; }
    }
}