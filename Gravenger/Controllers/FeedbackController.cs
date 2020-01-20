using System.Web;
using System.Web.Mvc;

namespace Gravenger.Controllers
{
    [Authorize]
    public class FeedbackController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}
