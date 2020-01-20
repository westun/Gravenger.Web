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
    public class HomeController : BaseController
    {
        private const string ViewNamePrefix = "feed_";
        private const string CurrentFeedVersion = "ver1.1";

        [HttpGet]
        public ActionResult Index(string version)
        {
            var viewName = $"{ViewNamePrefix}{ version ?? CurrentFeedVersion}";

            return View(viewName: viewName);
        }
    }
}
