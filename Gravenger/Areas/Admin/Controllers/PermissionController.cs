using Gravenger.Areas.Admin.ViewModels;
using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Can Manage Permissions")]
    public class PermissionController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public PermissionController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public string[] ExcludedRoles
        {
            get
            {
                return new string[]
                {
                    "Admin",
                    "Can Manage Account Permissions",
                    "Can Manage Permissions",
                };
            }
        }

        [HttpGet]
        public ActionResult ManageRoles()
        {
            var model = new ManageRolesVM
            {
                Roles = this._unitOfWork.Roles.GetAll().Select(r => new RoleVM { RoleID = r.RoleID, Name = r.Name }),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageRoles(ManageRolesVM model)
        {
            //TODO: Add validation

            if(model.NewRoleName != null)
            {
                var newRoleName = model.NewRoleName.Trim();
                if (this._unitOfWork.Roles.GetAll().Any(r => r.Name == newRoleName))
                {
                    ViewBag.ErrorMessage = $"A role with the name {newRoleName} already exists";
                    return View(model);
                }
                this._unitOfWork.Roles.Add(new Role { Name = newRoleName });
                this._unitOfWork.Complete();
            }

            return RedirectToAction("ManageRoles");
        }

        [HttpGet]
        [Authorize(Roles = "Can Manage Account Permissions")]
        public ActionResult ManageAccount(int selectedAccountID = 0)
        {
            PermissionManagementVM model = new PermissionManagementVM
            {
                Accounts = this._unitOfWork.Accounts.Find(a => a.Roles.Any(r => r.Name == "Admin")),
            };

            if (selectedAccountID > 0)
            {
                var account = this._unitOfWork.Accounts.GetWithRoles(selectedAccountID);
                if (account == null || !account.Roles.Any(r => r.Name == "Admin"))
                {
                    return RedirectToAction("ManageAccount");
                }

                model.SelectedAccount = account;

                var roles = this._unitOfWork.Roles.GetAll()
                    .Where(r => !this.ExcludedRoles.Contains(r.Name))
                    .OrderBy(r => r.Name);

                model.Roles = roles.Select(r => new RoleVM
                {
                    RoleID = r.RoleID,
                    Name = r.Name,
                    Mapped = account.Roles.Any(ar => ar.Name == r.Name),
                }).ToArray();
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Can Manage Account Permissions")]
        [ValidateAntiForgeryToken]
        public ActionResult ManageAccount(PermissionManagementVM model, int selectedAccountID)
        {
            int adminRoleID = this._unitOfWork.Roles
                .Find(r => r.Name.ToLower() == "admin")
                .Select(r => r.RoleID)
                .FirstOrDefault();

            var attemptingToMapAdminPermission = model.Roles.Any(r => r.RoleID == adminRoleID);
            if (!ModelState.IsValid || attemptingToMapAdminPermission)
            {
                return this.ManageAccount();
            }
            
            var account = this._unitOfWork.Accounts.GetWithRoles(selectedAccountID);
            var userRoleIds = (account.Roles ?? new Role[0]).Select(r => r.RoleID);
            foreach (var role in model.Roles)
            {
                bool alreadyMapped = userRoleIds.Contains(role.RoleID);
                bool add = !alreadyMapped && role.Mapped;
                bool remove = alreadyMapped && !role.Mapped;

                if (add)
                {
                    var dbRole = this._unitOfWork.Roles.Get(role.RoleID);
                    account.Roles.Add(dbRole);
                    this._unitOfWork.Complete();
                }

                if (remove)
                {
                    this._unitOfWork.Accounts.RemoveAccountRole(selectedAccountID, role.RoleID);
                    this._unitOfWork.Complete();
                }
            }

            return RedirectToAction("ManageAccount");
        }
    }
}