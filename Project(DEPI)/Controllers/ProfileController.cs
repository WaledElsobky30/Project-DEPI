using BookifyHotel.Data;
using BookifyHotel.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project_DEPI_.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly BookifyHotelDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(BookifyHotelDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var appUser = await _userManager.FindByIdAsync(userId);

            if (userProfile == null || appUser == null)
            {
                TempData["Error"] = "User profile data not found or incomplete.";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Email = appUser.Email;
            ViewBag.PhoneNumber = appUser.PhoneNumber;

            return View(userProfile);
        }
    }
}
