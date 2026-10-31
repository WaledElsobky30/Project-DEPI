using BookifyHotel.Data;
using BookifyHotel.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PLL.Services;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project_DEPI_.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly UserService _userService;
        private readonly RoomService _roomService;
        private readonly RoomTypeService _roomTypeService;
        private readonly BookifyHotelDbContext _context;
        private readonly IConfiguration _configuration;

        public BookingController(
            BookingService bookingService,
            UserService userService,
            RoomService roomService,
            RoomTypeService roomTypeService,
            BookifyHotelDbContext context,
            IConfiguration configuration)
        {
            _bookingService = bookingService;
            _userService = userService;
            _roomService = roomService;
            _roomTypeService = roomTypeService;
            _context = context;
            _configuration = configuration;
        }

        #region Admin Dashboard CRUD Operations
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingService.GetAllWithDetailsAsync();
            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetByIdWithDetailsAsync(id);
            if (booking == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (booking.UserId != currentUserId)
                {
                    return Forbid();
                }
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                await RefreshRoomOccupancyStatusAsync(booking.RoomId, excludeBookingId: booking.BookingId);
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Booking not found.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Index");
            }

            booking.Status = status;
            await _context.SaveChangesAsync();
            await RefreshRoomOccupancyStatusAsync(booking.RoomId);

            TempData["Success"] = $"Booking status updated to {status}.";
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                return await LoadCreateViewData(new Booking());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading data: {ex.Message}";
                return View(new Booking());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                if (booking.CheckInDate >= booking.CheckOutDate)
                {
                    ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
                }

                if (ModelState.IsValid)
                {
                    var room = await _context.Rooms.Include(r => r.RoomTypes).FirstOrDefaultAsync(r => r.RoomId == booking.RoomId);
                    if (room == null)
                    {
                        ModelState.AddModelError("RoomId", "Selected room not found.");
                        return await LoadCreateViewData(booking);
                    }
                    if (!room.IsAvailable || room.Status == "Maintenance")
                    {
                        ModelState.AddModelError("RoomId", "Selected room is currently out of service.");
                        return await LoadCreateViewData(booking);
                    }

                    var isAvailableForDates = await _bookingService.IsRoomAvailableForDatesAsync(
                        booking.RoomId, booking.CheckInDate, booking.CheckOutDate);

                    if (!isAvailableForDates)
                    {
                        ModelState.AddModelError("", "Selected room is already booked during those dates.");
                        return await LoadCreateViewData(booking);
                    }

                    var userProfile = await _context.UserProfiles.FindAsync(booking.UserProfileId);
                    if (userProfile != null)
                    {
                        booking.UserId = userProfile.UserId;
                    }

                    var nights = Math.Max((int)Math.Ceiling((booking.CheckOutDate - booking.CheckInDate).TotalDays), 1);
                    booking.TotalPrice = room.RoomTypes.PricePerNight * nights;
                    booking.BookingDate = DateTime.Now;
                    booking.Status = booking.Status ?? "Pending";

                    _bookingService.Create(booking);
                    await RefreshRoomOccupancyStatusAsync(room.RoomId);

                    TempData["Success"] = $"Booking created successfully! Total: ${booking.TotalPrice:N2}";
                    return RedirectToAction("Index");
                }
                return await LoadCreateViewData(booking);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating booking: {ex.Message}");
                return await LoadCreateViewData(booking);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.UserProfile)
                    .Include(b => b.Room)
                        .ThenInclude(r => r.RoomTypes)
                    .FirstOrDefaultAsync(b => b.BookingId == id);

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("Index");
                }
                return await LoadEditViewData(booking);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading booking: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Booking booking)
        {
            try
            {
                if (booking.CheckInDate >= booking.CheckOutDate)
                {
                    ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
                }

                if (ModelState.IsValid)
                {
                    var existingBooking = await _context.Bookings
                        .Include(b => b.Room)
                            .ThenInclude(r => r.RoomTypes)
                        .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

                    if (existingBooking == null)
                    {
                        TempData["Error"] = "Booking not found.";
                        return RedirectToAction("Index");
                    }

                    var previousRoomId = existingBooking.RoomId;

                    if (booking.Status != "Cancelled")
                    {
                        var isAvailableForDates = await _bookingService.IsRoomAvailableForDatesAsync(
                            booking.RoomId, booking.CheckInDate, booking.CheckOutDate, excludeBookingId: booking.BookingId);

                        if (!isAvailableForDates)
                        {
                            ModelState.AddModelError("", "The selected room has a conflicting booking for these dates.");
                            return await LoadEditViewData(booking);
                        }
                    }

                    // تحديث البيانات
                    existingBooking.UserProfileId = booking.UserProfileId;
                    existingBooking.RoomId = booking.RoomId;
                    existingBooking.CheckInDate = booking.CheckInDate;
                    existingBooking.CheckOutDate = booking.CheckOutDate;
                    existingBooking.Status = booking.Status;

                    var room = await _context.Rooms.Include(r => r.RoomTypes).FirstOrDefaultAsync(r => r.RoomId == booking.RoomId);
                    if (room != null)
                    {
                        var nights = Math.Max((int)Math.Ceiling((booking.CheckOutDate - booking.CheckInDate).TotalDays), 1);
                        existingBooking.TotalPrice = room.RoomTypes.PricePerNight * nights;
                    }

                    _bookingService.Update(existingBooking);

                    await RefreshRoomOccupancyStatusAsync(existingBooking.RoomId);
                    if (previousRoomId != existingBooking.RoomId)
                    {
                        await RefreshRoomOccupancyStatusAsync(previousRoomId);
                    }

                    TempData["Success"] = "Booking updated successfully!";
                    return RedirectToAction("Details", new { id = booking.BookingId });
                }
                return await LoadEditViewData(booking);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating booking: {ex.Message}");
                return await LoadEditViewData(booking);
            }
        }

        private async Task<ViewResult> LoadCreateViewData(Booking booking)
        {
            var users = await _context.UserProfiles.Select(u => new { u.Id, u.FullName, u.Email }).ToListAsync();
            var rooms = await _context.Rooms
                .Where(r => r.IsAvailable && r.Status != "Maintenance")
                .Include(r => r.RoomTypes)
                .Select(r => new { r.RoomId, r.RoomNumber, RoomType = r.RoomTypes.Name, PricePerNight = r.RoomTypes.PricePerNight })
                .ToListAsync();

            ViewBag.Users = new SelectList(users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.FullName} ({u.Email})" }), "Value", "Text");
            ViewBag.Rooms = new SelectList(rooms.Select(r => new SelectListItem { Value = r.RoomId.ToString(), Text = $"Room {r.RoomNumber} - {r.RoomType} (${r.PricePerNight:N2}/night)" }), "Value", "Text");
            ViewBag.TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.RoomsData = rooms.ToDictionary(r => r.RoomId.ToString(), r => new { r.RoomType, r.PricePerNight });

            return View("Create", booking);
        }

        private async Task<ViewResult> LoadEditViewData(Booking booking)
        {
            var users = await _context.UserProfiles.Select(u => new { u.Id, u.FullName, u.Email }).ToListAsync();
            var rooms = await _context.Rooms
                .Include(r => r.RoomTypes)
                .Select(r => new { r.RoomId, r.RoomNumber, RoomType = r.RoomTypes.Name, PricePerNight = r.RoomTypes.PricePerNight, IsAvailable = r.IsAvailable })
                .ToListAsync();

            ViewBag.Users = new SelectList(users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.FullName} ({u.Email})", Selected = u.Id == booking.UserProfileId }), "Value", "Text");
            ViewBag.Rooms = new SelectList(rooms.Select(r => new SelectListItem { Value = r.RoomId.ToString(), Text = $"Room {r.RoomNumber} - {r.RoomType} (${r.PricePerNight:N2}/night)" + (!r.IsAvailable && r.RoomId != booking.RoomId ? " [Maintenance]" : ""), Selected = r.RoomId == booking.RoomId }), "Value", "Text");
            ViewBag.TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.RoomsData = rooms.ToDictionary(r => r.RoomId.ToString(), r => new { r.RoomType, r.PricePerNight });

            return View("Edit", booking);
        }

        /// <summary>
        /// Updates Room.Status ("Booked" vs "Available") based on whether there is an active confirmed booking covering today,
        /// while keeping Room.IsAvailable true so future dates remain bookable.
        /// </summary>
        private async Task RefreshRoomOccupancyStatusAsync(int roomId, int? excludeBookingId = null)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null || room.Status == "Maintenance") return;

            var today = DateTime.Today;
            var isOccupiedToday = await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                (excludeBookingId == null || b.BookingId != excludeBookingId.Value) &&
                b.Status == "Confirmed" &&
                b.CheckInDate.Date <= today &&
                b.CheckOutDate.Date > today);

            room.Status = isOccupiedToday ? "Booked" : "Available";
            room.IsAvailable = true;
            await _context.SaveChangesAsync();
        }
        #endregion

        #region User Side Booking (Client Actions)

        [HttpGet]
        public IActionResult Checkout(int roomId)
        {
            var room = _context.Rooms.Include(r => r.RoomTypes).FirstOrDefault(r => r.RoomId == roomId);
            if (room == null) return NotFound();

            ViewBag.RoomId = room.RoomId;
            ViewBag.RoomNumber = room.RoomNumber;
            ViewBag.RoomType = room.RoomTypes.Name;
            ViewBag.PricePerNight = room.RoomTypes.PricePerNight;
            ViewBag.ImageUrl = room.ImageUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayNow(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomTypes)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null || booking.Status != "Pending")
            {
                TempData["Error"] = "Booking not found or already paid.";
                return RedirectToAction("MyBookings");
            }

            try
            {
                StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
                var domain = $"{this.Request.Scheme}://{this.Request.Host}";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    ClientReferenceId = booking.BookingId.ToString(),
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(booking.TotalPrice * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Pending Payment for Room {booking.Room.RoomNumber}",
                                    Description = $"Settlement for booking ID: {booking.BookingId}"
                                },
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = domain + $"/Booking/PaymentSuccess?bookingId={booking.BookingId}&paymentMethod=Credit%20Card&session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = domain + $"/Booking/MyBookings",
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error initiating payment: {ex.Message}";
                return RedirectToAction("MyBookings");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int roomId, DateTime checkIn, DateTime checkOut, string paymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            if (checkIn >= checkOut || checkIn < DateTime.Today)
            {
                TempData["Error"] = "Please select valid check-in and check-out dates.";
                return RedirectToAction("Checkout", new { roomId });
            }

            var room = await _context.Rooms.Include(r => r.RoomTypes).FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null || !room.IsAvailable || room.Status == "Maintenance")
            {
                TempData["Error"] = "Sorry, this room is currently unavailable.";
                return RedirectToAction("Index", "Home");
            }

            var isAvailable = await _bookingService.IsRoomAvailableForDatesAsync(roomId, checkIn, checkOut);
            if (!isAvailable)
            {
                TempData["Error"] = "This room is already booked for the selected dates.";
                return RedirectToAction("Checkout", new { roomId });
            }

            var nights = Math.Max((int)Math.Ceiling((checkOut - checkIn).TotalDays), 1);
            var totalPrice = nights * room.RoomTypes.PricePerNight;

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userProfile == null) return RedirectToAction("Register", "Register");

            try
            {
                var booking = new Booking
                {
                    RoomId = roomId,
                    UserProfileId = userProfile.Id,
                    UserId = userId,
                    BookingDate = DateTime.Now,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    TotalPrice = totalPrice,
                    Status = "Pending"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                if (paymentMethod == "Cash")
                {
                    booking.Status = "Confirmed";
                    var payment = new Payment
                    {
                        BookingId = booking.BookingId,
                        Amount = totalPrice,
                        PaymentDate = DateTime.Now,
                        PaymentMethod = "Cash",
                        PaymentStatus = "Pending",
                        TransactionRefrence = "CASH-ON-SITE"
                    };
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();
                    await RefreshRoomOccupancyStatusAsync(room.RoomId);

                    TempData["Success"] = "Booking Confirmed! Please pay upon arrival.";
                    return RedirectToAction("MyBookings");
                }

                if (paymentMethod == "Credit Card" || paymentMethod == "PayPal")
                {
                    StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
                    var domain = $"{this.Request.Scheme}://{this.Request.Host}";

                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        ClientReferenceId = booking.BookingId.ToString(),
                        LineItems = new List<SessionLineItemOptions>
                        {
                            new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    UnitAmount = (long)(totalPrice * 100),
                                    Currency = "usd",
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = $"Booking Room {room.RoomNumber}",
                                        Description = $"Stay from {checkIn:dd/MM/yyyy} to {checkOut:dd/MM/yyyy}"
                                    },
                                },
                                Quantity = 1,
                            },
                        },
                        Mode = "payment",
                        SuccessUrl = domain + $"/Booking/PaymentSuccess?bookingId={booking.BookingId}&paymentMethod={Uri.EscapeDataString(paymentMethod)}&session_id={{CHECKOUT_SESSION_ID}}",
                        CancelUrl = domain + $"/Booking/Checkout?roomId={roomId}",
                    };

                    var service = new SessionService();
                    Session session = await service.CreateAsync(options);

                    return Redirect(session.Url);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing booking: {ex.Message}";
                return RedirectToAction("Checkout", new { roomId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(int bookingId, string paymentMethod, string? session_id = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null) return NotFound();

            if (booking.Status == "Confirmed" && booking.Payments.Any(p => p.PaymentStatus == "Completed"))
            {
                TempData["Success"] = "Your booking is already confirmed.";
                return RedirectToAction("MyBookings");
            }

            if (string.IsNullOrWhiteSpace(session_id))
            {
                TempData["Error"] = "Invalid payment session.";
                return RedirectToAction("MyBookings");
            }

            try
            {
                StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(session_id);

                if (session == null || session.PaymentStatus != "paid")
                {
                    TempData["Error"] = "Payment could not be verified as completed.";
                    return RedirectToAction("MyBookings");
                }

                booking.Status = "Confirmed";
                _context.Bookings.Update(booking);

                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = booking.TotalPrice,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "Completed",
                    TransactionRefrence = session.PaymentIntentId ?? session.Id
                };
                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();
                await RefreshRoomOccupancyStatusAsync(booking.RoomId);

                TempData["Success"] = "Payment Successful! Your booking is confirmed.";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not verify payment session: {ex.Message}";
                return RedirectToAction("MyBookings");
            }
        }

        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomTypes)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }
        #endregion
    }
}
