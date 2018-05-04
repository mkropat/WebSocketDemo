using Microsoft.AspNetCore.Mvc;

namespace WebSocketDemo.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
