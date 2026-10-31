using Microsoft.AspNetCore.Mvc;

namespace Project_DEPI_.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
