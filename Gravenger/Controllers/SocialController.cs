using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Controllers
{
    [Authorize]
    public class SocialController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Boards()
        {
            return View();
        }

        [HttpGet]
        public ActionResult _originalBoards()
        {
            return View();
        }
    }
}
