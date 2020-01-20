using Gravenger.Areas.Admin.ViewModels;
using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Security.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Can Create Invitations")]
    public class InvitationController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public InvitationController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        
        [HttpGet]
        public ActionResult Invite()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invite(InviteVM model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            model.Email = model.Email.Trim();
            model.FirstName = model.FirstName.Trim();
            model.LastName = model.LastName.Trim();

            var existingInvitation = this._unitOfWork.Invitations.GetByEmail(model.Email);
            if(existingInvitation != null)
            {
                ViewBag.ErrorMessage = $"The email address { model.Email } has already been invited.";
                return View(model);
            }

            var currentAccountID = this.User.Identity.GetAccountID();
            this._unitOfWork.Invitations.Add(new Invitation
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                CreatedByAccountID = currentAccountID,
            });

            this._unitOfWork.EmailVerifications.Add(new EmailVerification {
                Email = model.Email,
                CreatedByAccountID = currentAccountID,
                //TODO: consider encapsulating this behavior in the "EmailVerification" class
                Expires = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(28)),
            });

            this._unitOfWork.Complete();

            ViewBag.SuccessMessage = $"Invitation created for {model.Email}.";

            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            var invitations = this._unitOfWork.Invitations.GetAll().OrderBy(i => i.LastName);

            var model = new InvitationListVM
            {
                Invitations = invitations.Select(i => new InvitationVM
                {
                    InvitationID = i.InvitationID,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    Email = i.Email,
                    Accepted = i.Accepted,
                    LastSentDate = this.GetLastSentDateString(i.SentDate),
                    CreatedDate = i.CreatedDate,
                    CreatedByAccountID = i.CreatedByAccountID,
                    CreatedByAccount = this._unitOfWork.Accounts.Get(i.CreatedByAccountID),
                }).ToArray()
            };

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendInvitation(int invitationID)
        {
            var invitation = this._unitOfWork.Invitations.Get(invitationID);
            if(invitation == null)
            {
                return RedirectToAction("list");
            }

            var emailVerification = new EmailVerification
            {
                Email = invitation.Email,
                CreatedByAccountID = this.User.Identity.GetAccountID(),
                //TODO: consider encapsulating this behavior in the "EmailVerification" class
                Expires = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(28)),
            };

            this._unitOfWork.EmailVerifications.Add(emailVerification);
            this._unitOfWork.Complete();

            //TODO: update this to by dynamic or global
            string acceptInvitationPath = Url.Action($"accept", "invitation", new
            {
                area = string.Empty,
                uid = emailVerification.EmailVerificationID,
                tkn = emailVerification.Token
            });

            string hostUrl = this.GetHostUrl(this.Request);
            string invitationLink = $"{hostUrl}{acceptInvitationPath}";
            
            string mailTo = invitation.Email;
            string subject = "Invitation to join Seenry";
            string body = $"Click this link to accept your invitation: {invitationLink.Replace("&", "%26")}";

            string mailToUrl = $"mailto:{mailTo}?subject={subject}&body={body}";

            return Redirect(mailToUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkInvitationSent(int invitationID)
        {
            var invitation = this._unitOfWork.Invitations.Get(invitationID);
            if (invitation == null)
            {
                return RedirectToAction("list");
            }

            invitation.MarkSent();
            this._unitOfWork.Complete();

            TempData["SuccessMessage"] = $"Invitation marked sent for {invitation.Email}.";

            return this.RedirectToAction("list");
        }

        private string GetHostUrl(HttpRequestBase requestContext)
        {
            return requestContext.Url.GetLeftPart(UriPartial.Authority);
        }

        private string GetLastSentDateString(DateTimeOffset? lastSentDate)
        {
            if (!lastSentDate.HasValue)
            {
                return null;
            }
            
            return lastSentDate.Value.ToString("MM/dd/yyyy");
        }
    }
}