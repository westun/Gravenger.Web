using Gravenger.Domain.Core;
using Gravenger.Domain.Security.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Controllers
{
    [Authorize]
    [RoutePrefix("profile")]
    public class ProfileController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProfileController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Index()
        {
            int currentAccountID = this.User.Identity.GetAccountID();
            ViewBag.AccountID = currentAccountID;
            ViewBag.CurrentLoggedInAccountID = currentAccountID;

            return View();
        }

        [HttpGet]
        [Route("display/{username}")]
        public ActionResult Display(string username)
        {
            if(username != null)
            {
                var account = this._unitOfWork.Accounts.GetByUsername(username);
                if(account == null)
                {
                    return RedirectToAction("index");
                }

                ViewBag.AccountID = account.AccountID;
                ViewBag.CurrentLoggedInAccountID = this.User.Identity.GetAccountID();
            }

            return View("index");
        }

        [HttpGet]
        public ActionResult UsingKnockout()
        {
            return View();
        }

        [HttpGet]
        public ActionResult UserProfile()
        {
            return View();
        }
    }
}