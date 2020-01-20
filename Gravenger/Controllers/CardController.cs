using System.Web;
using System.Web.Mvc;

namespace Gravenger.Controllers
{
    [Authorize]
    public class CardController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult VueJS()
        {
            return View();
        }
    }
}
