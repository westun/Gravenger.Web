using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Core.Providers;
using Gravenger.Domain.Core.Security;
using Gravenger.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IAuthProvider _authProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordEncryptor _passwordEncryptor;
        private bool inviteOnly = System.Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["InvitationOnly"].ToString());

        public AccountController(IUnitOfWork unitOfWork, IAuthProvider authProvider, IPasswordEncryptor passwordEncryptor)
        {
            this._authProvider = authProvider;
            this._unitOfWork = unitOfWork;
            this._passwordEncryptor = passwordEncryptor;
        }
                
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            if (this.SignedIn)
            {
                return Redirect("~/");
            }

            var model = new AccountVM
            {
                ReturnUrl = returnUrl
            };
            
            return View("index", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return RedirectToAction("index");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountVM model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                this.ClearAccountVMPasswordAndModelState(model);
                ViewBag.ErrorMessage = "Username and/or password is in an invalid format.";

                return View("index", model);
            }
            
            if (this._authProvider.Authenticate(model.AccountLoginVM.Username, model.AccountLoginVM.Password, createPersistentCookie: true))
            {
                return Redirect(!string.IsNullOrEmpty(returnUrl) ? returnUrl : Url.Action("Index", "Home"));
            }

            this.ClearAccountVMPasswordAndModelState(model);
            ViewBag.ErrorMessage = "Invalid login or password.";

            return View("index", model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Create()
        {
            return RedirectToAction("index");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AccountVM model)
        {
            if (this.inviteOnly)
            {
                return this.RedirectToAction("index");
            }

            if (!ModelState.IsValid)
            {
                this.ClearAccountVMPasswordAndModelState(model);
                ViewBag.ErrorMessage = "Username and/or password is in an invalid format.";

                return View("index", model);
            }
            
            Account account = new Account
            {
                //TODO: Does name, email, username strings need to be auto formated for proper upper/lower casing, etc?
                Name = model.AccountCreateVM.Name.Trim(),
                Email = model.AccountCreateVM.Email.Trim(),
                Username = model.AccountCreateVM.Username.Trim(),
                AccountCredentials = new AccountCredential[]
                {
                    new AccountCredential { Password = model.AccountCreateVM.Password },
                }
            };

            string errorMessage = string.Empty;
            if (this._unitOfWork.Accounts.CreateAccount(account, out errorMessage) != null)
            {
                this._authProvider.Authenticate(model.AccountCreateVM.Username, model.AccountCreateVM.Password);
                return Redirect("~/");
            }

            this.ClearAccountVMPasswordAndModelState(model);
            ViewBag.ErrorMessage = errorMessage;

            return View("index", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Reset()
        {
            return View(new AccountPasswordResetVM());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reset(AccountPasswordResetVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var accountByUsername = this._unitOfWork.Accounts.GetByUsername(model.UsernameOrEmail);
            var accountByEmail = this._unitOfWork.Accounts.GetByEmail(model.UsernameOrEmail);
            bool accountFound = accountByUsername != null || accountByEmail != null;
            if (accountFound)
            {
                var account = accountByUsername ?? accountByEmail;

                var revocableTokens = this._unitOfWork.PasswordResetRequests.GetAllRevocableTokens(account.AccountID);
                foreach (var token in revocableTokens)
                {
                    token.Revoke();
                }

                var resetRequest = new PasswordResetRequest(account.AccountID);
                this._unitOfWork.PasswordResetRequests.Add(resetRequest);

                this._unitOfWork.Complete();

                if (!this.HttpContext.Request.IsLocal)
                {
                    await this.SendPasswordResetMailAsync(account.Email, resetRequest);
                }
            }

            ViewBag.SuccessMessage = "If you have an account, an email will be sent to your email address.";

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyReset(int uid, string tkn)
        {
            var resetRequest = this._unitOfWork.PasswordResetRequests.GetByIDAndToken(uid, tkn);
            if (resetRequest == null)
            {
                return this.HttpNotFound();
            }

            if (!resetRequest.IsValid)
            {
                //todo: add tempdata to let user know to try to reset password again
                return this.RedirectToAction("reset");
            }

            return this.View(new AccountVerifyResetVM());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyReset(AccountVerifyResetVM model, int uid, string tkn)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resetRequest = this._unitOfWork.PasswordResetRequests.GetByIDAndToken(uid, tkn);
            if (resetRequest == null)
            {
                return this.HttpNotFound();
            }

            if (!resetRequest.IsValid)
            {
                //todo: add tempdata to let user know to try to reset password again
                return this.RedirectToAction("reset");
            }

            resetRequest.MarkUsed();

            //TODO: this is using account "AccountCredentials" property to access credentials from database.  Should credentials have their own repository?
            //TODO: this needs to be abstracted out of this controller or something
            var account = this._unitOfWork.Accounts.GetWithCredentials(resetRequest.AccountID);
            foreach (var credential in account.AccountCredentials.Where(ac => ac.Active))
            {
                credential.Active = false;
            }

            var salt = this._passwordEncryptor.GenerateSalt();
            var newCredentials = new AccountCredential
            {
                Active = true,
                Salt = salt,
                Password = this._passwordEncryptor.HashPassword(model.Password, salt),
                Version = System.Configuration.ConfigurationManager.AppSettings["Authentication.Version"].ToString(),
            };

            account.AccountCredentials.Add(newCredentials);

            this._unitOfWork.Complete();

            return this.View("ResetSuccess");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            this._authProvider.Logout();

            return RedirectToAction("Index");
        }

        private async Task SendPasswordResetMailAsync(string toEmail, PasswordResetRequest emailVerification)
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["Sendgrid.Key"].ToString();
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@gravenger.app", "Gravenger Support");
            var subject = "Reset request for Gravenger";
            var to = new EmailAddress(toEmail);

            int uid = emailVerification.PasswordResetRequestID; 
            string token = emailVerification.Token;

            string passwordResetPath = Url.Action("VerifyReset", "Account", new
            {
                area = string.Empty,
                uid = uid,
                tkn = token
            });

            string hostUrl = this.GetHostUrl(this.Request);
            string passwordResetLink = $"{hostUrl}{passwordResetPath}";

            var plainTextContent = $"You have requested to reset your password.  Use this link to reset your password: {passwordResetLink}";
            var htmlContent = $"You have requested to reset your password.  Use this link to reset your password: <a href='{passwordResetLink}'>{passwordResetLink}</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        private void ClearAccountVMPasswordAndModelState(AccountVM model)
        {
            if (model?.AccountLoginVM?.Password != null)
            {
                model.AccountLoginVM.Password = null;
            }

            if (model?.AccountCreateVM?.Password != null)
            {
                model.AccountCreateVM.Password = null;
            }

            this.ModelState.Clear();
        }

        private string GetHostUrl(HttpRequestBase requestContext)
        {
            return requestContext.Url.GetLeftPart(UriPartial.Authority);
        }
    }
}
