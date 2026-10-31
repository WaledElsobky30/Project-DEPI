using BookifyHotel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_DEPI_.Models;
using System.Diagnostics;

namespace BookifyHotel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookifyHotelDbContext _context; 

        public HomeController(ILogger<HomeController> logger, BookifyHotelDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 3. بنجيب الغرف المتاحة فقط، وبناخد أول 3 كمثال (للعرض)
            var availableRooms = _context.Rooms
                                         .Include(r => r.RoomTypes) // لازم نعمل Include عشان السعر والنوع
                                         .Where(r => r.IsAvailable && r.Status == "Available")
                                         .Take(6) // بنعرض 6 غرف بس في الرئيسية
                                         .ToList();

            return View(availableRooms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}