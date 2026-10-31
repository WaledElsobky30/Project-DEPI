//using BookifyHotel.Model;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using PLL.Services;
//using System.Security.Claims;

//namespace Project_DEPI_.Controllers
//{
//    [Authorize]
//    public class BokingController : Controller
//    {
//        private readonly BookingService _bookingService;
//        private readonly RoomService _roomService;
//        private readonly IHttpContextAccessor _httpContextAccessor;
            
//        public BokingController(
//            BookingService bookingService,
//            RoomService roomService,
//            IHttpContextAccessor httpContextAccessor)
//        {
//            _bookingService = bookingService;
//            _roomService = roomService;
//            _httpContextAccessor = httpContextAccessor;
//        }

//        // Action علشان اليوزر يبص على الحجوزات القديمة
//        public IActionResult Index()
//        { 
//            var userId = GetCurrentUserId();
//            var bookings = _bookingService.GetBookingsByUserId(userId);

//            return View(bookings);
//        }

//        // Action لعرض صفحة الحجز للغرفة المحددة
//        [HttpGet]
//        public IActionResult Book(int id) // id هنا هو RoomId
//        {
//            var room = _roomService.GetById(id);
//            if (room == null || !room.IsAvailable)
//            {
//                TempData["Error"] = "Room is not available";
//                return RedirectToAction("Index", "Room");
//            }

//            var model = new BookingViewModel
//            {
//                RoomId = id,
//                RoomNumber = room.RoomNumber,
//                RoomType = room.RoomTypes?.Name ?? "Standard",
//                PricePerNight = room.RoomTypes?.PricePerNight ?? 0,
//                MaxGuests = GetMaxGuests(room.RoomTypes?.Name),
//                MinCheckInDate = DateTime.Today.AddDays(1),
//                MaxCheckOutDate = DateTime.Today.AddDays(90)
//            };

//            return View(model);
//        }

//        // Action لتأكيد الحجز
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Book(BookingViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            var userId = GetCurrentUserId();
//            var room = _roomService.GetById(model.RoomId);

//            // التحقق من توفر الغرفة
//            if (room == null || !room.IsAvailable)
//            {
//                ModelState.AddModelError("", "Room is not available");
//                return View(model);
//            }

//            // التحقق من صحة التواريخ
//            if (model.CheckInDate >= model.CheckOutDate)
//            {
//                ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date");
//                return View(model);
//            }

//            if (model.CheckInDate < DateTime.Today.AddDays(1))
//            {
//                ModelState.AddModelError("CheckInDate", "Check-in date must be at least tomorrow");
//                return View(model);
//            }

//            // التحقق من توفر الغرفة في التواريخ المطلوبة
//            if (!_bookingService.IsRoomAvailableForDates(model.RoomId, model.CheckInDate, model.CheckOutDate))
//            {
//                ModelState.AddModelError("", "Room is not available for the selected dates");
//                return View(model);
//            }

//            try
//            {
//                // حساب عدد الليالي والسعر الكلي
//                int nights = (int)(model.CheckOutDate - model.CheckInDate).TotalDays;
//                decimal totalPrice = room.RoomTypes.PricePerNight * nights;

//                // إنشاء الحجز
//                var booking = new Booking
//                {
//                    UserId = userId,
//                    RoomId = model.RoomId,
//                    CheckInDate = model.CheckInDate,
//                    CheckOutDate = model.CheckOutDate,
//                    TotalPrice = totalPrice,
//                    Status = "Confirmed",
//                    BookingDate = DateTime.Now
//                };

//                // تحديث حالة الغرفة
//                room.IsAvailable = false;
//                _roomService.Update(room);

//                _bookingService.Create(booking);

//                TempData["Success"] = $"Booking confirmed successfully! Total: ${totalPrice:N0}";
//                return RedirectToAction("Confirmation", new { id = booking.BookingId });
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
//                return View(model);
//            }
//        }
        

//        // صفحة تأكيد الحجز
//        public IActionResult Confirmation(int id)
//        {
//            var booking = _bookingService.GetBookingWithRoom(id);
//            var userId = GetCurrentUserId();

//            if (booking == null || booking.UserId != userId)
//            {
//                TempData["Error"] = "Booking not found";
//                return RedirectToAction("Index");
//            }

//            return View(booking);
//        }

//        // إلغاء الحجز
//        [HttpPost]
//        public IActionResult Cancel(int id)
//        {
//            var booking = _bookingService.GetById(id);
//            var userId = GetCurrentUserId();

//            if (booking == null || booking.UserId != userId)
//            {
//                return Json(new { success = false, message = "Booking not found" });
//            }

//            if (booking.Status != "Confirmed")
//            {
//                return Json(new { success = false, message = "Cannot cancel this booking" });
//            }

//            // تحديث حالة الحجز
//            booking.Status = "Cancelled";
//            _bookingService.Update(booking);

//            // تحديث حالة الغرفة
//            var room = _roomService.GetById(booking.RoomId);
//            if (room != null)
//            {
//                room.IsAvailable = true;
//                _roomService.Update(room);
//            }

//            return Json(new { success = true, message = "Booking cancelled successfully" });
//        }

//        // Helper Methods
//        private int GetCurrentUserId()
//        {
//            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
//            return int.Parse(userIdClaim?.Value);
//        }

//        private int GetMaxGuests(string roomType)
//        {
//            return roomType?.ToLower() switch
//            {
//                "single room" => 1,
//                "double room" => 2,
//                "suite" => 2,
//                "royal suite" => 3,
//                "family room" => 4,
//                _ => 2
//            };
//        }
//    }
//}
