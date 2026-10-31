using BookifyHotel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_DEPI_.Models;
using System.Diagnostics;
using System.Linq;

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
            ViewBag.RoomTypes = _context.RoomTypes.OrderBy(rt => rt.PricePerNight).ToList();

            var availableRooms = _context.Rooms
                .Include(r => r.RoomTypes)
                .Where(r => r.IsAvailable && r.Status != "Maintenance")
                .OrderBy(r => r.RoomNumber)
                .Take(6)
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
