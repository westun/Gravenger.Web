using Gravenger.Domain.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gravenger.Areas.Admin.ViewModels
{
    public class PermissionManagementVM
    {
        public IEnumerable<Account> Accounts { get; set; }
        public Account SelectedAccount { get; set; }
        public int SelectedAccountID { get; set; }
        public RoleVM[] Roles { get; set; }
    }
}