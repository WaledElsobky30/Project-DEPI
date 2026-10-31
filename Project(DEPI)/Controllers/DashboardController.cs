using BookifyHotel.Data;
using DAL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookifyHotel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly BookifyHotelDbContext _context;

        public DashboardController(BookifyHotelDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // الغرف
            ViewBag.TotalRooms = await _context.Rooms.CountAsync();
            ViewBag.AvailableRooms = await _context.Rooms.CountAsync(r => r.IsAvailable && r.Status == "Available");

            // الحجوزات
            ViewBag.TodaysBookings = await _context.Bookings
                .CountAsync(b => b.CheckInDate.Date == today);

            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.ConfirmedBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");

            // المستخدمين
            ViewBag.TotalUsers = await _context.UserProfiles.CountAsync();

            // الإيرادات 
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            ViewBag.MonthlyRevenue = await _context.Bookings
                .Where(b => b.CheckInDate >= firstDayOfMonth && b.CheckInDate <= lastDayOfMonth &&
                           (b.Status == "Confirmed" || b.Status == "Completed"))
                .SumAsync(b => (double?)b.TotalPrice) ?? 0;

            // الحجوزات الجديده
            ViewBag.RecentBookings = await _context.Bookings
                .Include(b => b.UserProfile)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomTypes)
                .OrderByDescending(b => b.CheckInDate)
                .Take(5)
                .Select(b => new BookingViewModel
                {
                    BookingId = b.BookingId,
                    FullName = b.UserProfile.FullName,
                    RoomNumber = b.Room.RoomNumber,
                    RoomType = b.Room.RoomTypes.Name,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status,
                    TotalPrice = b.TotalPrice
                })
                .ToListAsync();

            return View();
        }
    }
}
