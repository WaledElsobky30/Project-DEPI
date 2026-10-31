using Microsoft.AspNetCore.Mvc;

namespace Project_DEPI_.Controllers
{
    public class BasePageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
