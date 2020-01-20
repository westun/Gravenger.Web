using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Core.Providers;
using Gravenger.ViewModels;
using System.Web.Mvc;

namespace Gravenger.Controllers
{
    [Authorize]
    public class InvitationController : BaseController
    {
        private readonly IAuthProvider _authProvider;
        private readonly IUnitOfWork _unitOfWork;

        public InvitationController(IUnitOfWork unitOfWork, IAuthProvider authProvider)
        {
            this._authProvider = authProvider;
            this._unitOfWork = unitOfWork;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (this.SignedIn)
            {
                return this.Redirect("~/");
            }

            //TODO: Once the site has the ability to send transactional mails, 
            //allow this page to be seen and do not redirect
            return this.Redirect("~/");

            //return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(InvitationVM model)
        {
            //TODO: Once the site has the ability to send transactional mails, 
            //if a valid invitation is found by email
            //create new email verification and resend invitation mail

            return this.View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Accept(int uid, string tkn)
        {
            if (this.SignedIn)
            {
                return this.Redirect("~/");
            }

            var emailVerification = this._unitOfWork.EmailVerifications.GetByIDAndToken(uid, tkn);
            if (emailVerification == null || !emailVerification.IsValid)
            {
                return this.InvalidInvitationRedirect();
            }

            var invitation = this._unitOfWork.Invitations.GetByEmail(emailVerification.Email);
            if (invitation == null || invitation.Accepted)
            {
                return this.InvalidInvitationRedirect();
            }

            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Accept(InvitationAcceptVM model, int uid, string tkn)
        {
            if (!this.ModelState.IsValid)
            {
                this.ClearInvitationAcceptVMPasswordAndModelState(model);
                ViewBag.ErrorMessage = "Username and/or password is in an invalid format.";

                return this.View(model);
            }

            var emailVerification = this._unitOfWork.EmailVerifications.GetByIDAndToken(uid, tkn);
            if (emailVerification == null || !emailVerification.IsValid)
            {
                return this.InvalidInvitationRedirect();
            }

            var emailMatchesVerification = model.AccountCreateVM.Email.ToLower() == emailVerification.Email.ToLower();
            if (!emailMatchesVerification)
            {
                this.ClearInvitationAcceptVMPasswordAndModelState(model);
                ViewBag.ErrorMessage = "Please use the email address your invitation was sent to.";

                return this.View(model);
            }

            var invitation = this._unitOfWork.Invitations.GetByEmail(emailVerification.Email);
            if (invitation == null || invitation.Accepted)
            {
                return this.InvalidInvitationRedirect();
            }

            var newAccount = this.CreateAccountFromViewModel(model.AccountCreateVM);

            string errorMessage = string.Empty;
            if (this._unitOfWork.Accounts.CreateAccount(newAccount, out errorMessage) != null)
            {
                emailVerification.Used = true;
                emailVerification.AccountID = newAccount.AccountID;

                invitation.Accepted = true;
                invitation.AccountID = newAccount.AccountID;

                this._unitOfWork.Complete();

                this._authProvider.Authenticate(model.AccountCreateVM.Username, model.AccountCreateVM.Password);

                return this.Redirect("~/profile");
            }

            this.ClearInvitationAcceptVMPasswordAndModelState(model);
            ViewBag.ErrorMessage = errorMessage;

            return this.View(model);
        }

        private ActionResult InvalidInvitationRedirect()
        {
            return this.RedirectToAction("index");
        }

        private Account CreateAccountFromViewModel(AccountCreateVM viewModel)
        {
            return new Account
            {
                Name = viewModel.Name,
                Email = viewModel.Email,
                Username = viewModel.Username,
                AccountCredentials = new AccountCredential[]
                {
                    new AccountCredential { Password = viewModel.Password },
                }
            };
        }

        private void ClearInvitationAcceptVMPasswordAndModelState(InvitationAcceptVM model)
        {
            if (model?.AccountCreateVM?.Password != null)
            {
                model.AccountCreateVM.Password = null;
            }

            this.ModelState.Clear();
        }
    }
}
