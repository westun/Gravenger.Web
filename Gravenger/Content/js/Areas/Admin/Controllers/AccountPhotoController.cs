using System.Web;
using System.Web.Mvc;

namespace Gravenger.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Can Manage Account Photos")]
    public class AccountPhotoController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}