using Gravenger.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Can Manage Cards")]
    public class CardController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public CardController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}