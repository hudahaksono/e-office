using System.Web.Mvc;

namespace Surat.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Forbidden()
        {
            return View();
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

        public ActionResult InternalServerError()
        {
            return View();
        }
	}
}