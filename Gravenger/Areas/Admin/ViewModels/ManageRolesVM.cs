using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gravenger.Areas.Admin.ViewModels
{
    public class ManageRolesVM
    {
        public IEnumerable<RoleVM> Roles { get; set; }

        [Required]
        [Display(Name = "Role Name")]
        public string NewRoleName { get; set; }
    }
}