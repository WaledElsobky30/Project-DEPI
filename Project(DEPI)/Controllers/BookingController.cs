using BookifyHotel.Data;
using BookifyHotel.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PLL.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace Project_DEPI_.Controllers
{
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
        public IActionResult Index()
        {
            var Booking = _bookingService.GetAll();
            return View(Booking);
        }

        public IActionResult Details(int id)
        {
            var booking = _bookingService.GetById(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                await UpdateRoomStatus(booking.RoomId, "Cancelled");
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

        public async Task<IActionResult> Create()
        {
            try
            {
                var users = await _context.UserProfiles.Select(u => new { u.Id, u.FullName, u.Email }).ToListAsync();
                var userList = users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.FullName} ({u.Email})" }).ToList();

                var availableRooms = await _context.Rooms
                    .Where(r => r.IsAvailable && r.Status == "Available")
                    .Include(r => r.RoomTypes)
                    .Select(r => new { r.RoomId, r.RoomNumber, RoomType = r.RoomTypes.Name, PricePerNight = r.RoomTypes.PricePerNight })
                    .ToListAsync();

                var roomList = availableRooms.Select(r => new SelectListItem { Value = r.RoomId.ToString(), Text = $"Room {r.RoomNumber} - {r.RoomType} (${r.PricePerNight:N2}/night)" }).ToList();

                ViewBag.Users = new SelectList(userList, "Value", "Text");
                ViewBag.Rooms = new SelectList(roomList, "Value", "Text");
                ViewBag.TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
                ViewBag.RoomsData = availableRooms.ToDictionary(r => r.RoomId.ToString(), r => new { r.RoomType, r.PricePerNight });

                return View(new Booking());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading data: {ex.Message}";
                return View(new Booking());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var room = await _context.Rooms.Include(r => r.RoomTypes).FirstOrDefaultAsync(r => r.RoomId == booking.RoomId);
                    if (room == null) { ModelState.AddModelError("RoomId", "Selected room not found."); return await LoadCreateViewData(booking); }
                    if (!room.IsAvailable) { ModelState.AddModelError("RoomId", "Selected room is not available."); return await LoadCreateViewData(booking); }

                    var conflictingBooking = await _context.Bookings
                        .Where(b => b.RoomId == booking.RoomId && b.Status != "Cancelled")
                        .Where(b => (booking.CheckInDate >= b.CheckInDate && booking.CheckInDate < b.CheckOutDate) ||
                                   (booking.CheckOutDate > b.CheckInDate && booking.CheckOutDate <= b.CheckOutDate) ||
                                   (booking.CheckInDate <= b.CheckInDate && booking.CheckOutDate >= b.CheckOutDate))
                        .FirstOrDefaultAsync();

                    if (conflictingBooking != null) { ModelState.AddModelError("", $"Room is already booked from {conflictingBooking.CheckInDate:MMM dd} to {conflictingBooking.CheckOutDate:MMM dd}"); return await LoadCreateViewData(booking); }

                    var nights = (int)Math.Ceiling((booking.CheckOutDate - booking.CheckInDate).TotalDays);
                    booking.TotalPrice = room.RoomTypes.PricePerNight * nights;
                    booking.BookingDate = DateTime.Now;
                    booking.Status = booking.Status ?? "Pending";

                    _bookingService.Create(booking);

                    if (booking.Status == "Confirmed")
                    {
                        room.IsAvailable = false;
                        room.Status = "Booked";
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = $"Booking created successfully! Total: ${booking.TotalPrice:N2}";
                    return RedirectToAction("Index");
                }
                return await LoadCreateViewData(booking);
            }
            catch (Exception ex) { ModelState.AddModelError("", $"Error creating booking: {ex.Message}"); return await LoadCreateViewData(booking); }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var booking = await _context.Bookings.Include(b => b.UserProfile).Include(b => b.Room).ThenInclude(r => r.RoomTypes).FirstOrDefaultAsync(b => b.BookingId == id);
                if (booking == null) { TempData["Error"] = "Booking not found."; return RedirectToAction("Index"); }
                return await LoadEditViewData(booking);
            }
            catch (Exception ex) { TempData["Error"] = $"Error loading booking: {ex.Message}"; return RedirectToAction("Index"); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Booking booking)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingBooking = await _context.Bookings.Include(b => b.Room).ThenInclude(r => r.RoomTypes).FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);
                    if (existingBooking == null) { TempData["Error"] = "Booking not found."; return RedirectToAction("Index"); }

                    if (existingBooking.RoomId != booking.RoomId)
                    {
                        var newRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == booking.RoomId);
                        if (newRoom == null || (!newRoom.IsAvailable && booking.Status != "Cancelled")) { ModelState.AddModelError("RoomId", "Selected room is not available."); return await LoadEditViewData(booking); }
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
                        var nights = (int)Math.Ceiling((booking.CheckOutDate - booking.CheckInDate).TotalDays);
                        existingBooking.TotalPrice = room.RoomTypes.PricePerNight * nights;
                    }

                    await UpdateRoomStatus(existingBooking.RoomId, booking.Status);
                    _bookingService.Update(existingBooking);

                    TempData["Success"] = "Booking updated successfully!";
                    return RedirectToAction("Details", new { id = booking.BookingId });
                }
                return await LoadEditViewData(booking);
            }
            catch (Exception ex) { ModelState.AddModelError("", $"Error updating booking: {ex.Message}"); return await LoadEditViewData(booking); }
        }

        private async Task<ViewResult> LoadCreateViewData(Booking booking)
        {
            var users = await _context.UserProfiles.Select(u => new { u.Id, u.FullName, u.Email }).ToListAsync();
            var rooms = await _context.Rooms.Where(r => r.IsAvailable).Include(r => r.RoomTypes).Select(r => new { r.RoomId, r.RoomNumber, RoomType = r.RoomTypes.Name, PricePerNight = r.RoomTypes.PricePerNight }).ToListAsync();

            ViewBag.Users = new SelectList(users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.FullName} ({u.Email})" }), "Value", "Text");
            ViewBag.Rooms = new SelectList(rooms.Select(r => new SelectListItem { Value = r.RoomId.ToString(), Text = $"Room {r.RoomNumber} - {r.RoomType} (${r.PricePerNight:N2}/night)" }), "Value", "Text");
            ViewBag.TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.RoomsData = rooms.ToDictionary(r => r.RoomId.ToString(), r => new { r.RoomType, r.PricePerNight });

            return View(booking);
        }

        private async Task<ViewResult> LoadEditViewData(Booking booking)
        {
            var users = await _context.UserProfiles.Select(u => new { u.Id, u.FullName, u.Email }).ToListAsync();
            var rooms = await _context.Rooms.Include(r => r.RoomTypes).Select(r => new { r.RoomId, r.RoomNumber, RoomType = r.RoomTypes.Name, PricePerNight = r.RoomTypes.PricePerNight, IsAvailable = r.IsAvailable }).ToListAsync();

            ViewBag.Users = new SelectList(users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.FullName} ({u.Email})", Selected = u.Id == booking.UserProfileId }), "Value", "Text");
            ViewBag.Rooms = new SelectList(rooms.Select(r => new SelectListItem { Value = r.RoomId.ToString(), Text = $"Room {r.RoomNumber} - {r.RoomType} (${r.PricePerNight:N2}/night)" + (!r.IsAvailable && r.RoomId != booking.RoomId ? " [Not Available]" : ""), Selected = r.RoomId == booking.RoomId }), "Value", "Text");
            ViewBag.TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.RoomsData = rooms.ToDictionary(r => r.RoomId.ToString(), r => new { r.RoomType, r.PricePerNight });

            return View(booking);
        }

        private async Task UpdateRoomStatus(int roomId, string bookingStatus)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                if (bookingStatus == "Confirmed") { room.IsAvailable = false; room.Status = "Booked"; }
                else if (bookingStatus == "Cancelled" || bookingStatus == "Completed") { room.IsAvailable = true; room.Status = "Available"; }
                await _context.SaveChangesAsync();
            }
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
            if (userId == null) return RedirectToAction("Login", "Account");

            // بجيب الحجز
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomTypes)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            // بعد كده اتشيك عليه
            if (booking == null || booking.Status != "Pending")
            {
                TempData["Error"] = "Booking not found or already paid.";
                return RedirectToAction("MyBookings");
            }

            try
            {
                //هنا هبدأ اظبط عملية الدفع بتاعت Stripe
                StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
                var domain = $"{this.Request.Scheme}://{this.Request.Host}";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
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
                    //دي بترجع تم الدفع وكده 
                    SuccessUrl = domain + $"/Booking/PaymentSuccess?bookingId={booking.BookingId}&paymentMethod=Credit Card",
                    CancelUrl = domain + $"/Booking/MyBookings",
                };

                var service = new SessionService();
                Session session = service.Create(options);

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
            if (userId == null) return RedirectToAction("Login", "Account");

            if (checkIn >= checkOut || checkIn < DateTime.Today)
            {
                TempData["Error"] = "Please select valid dates.";
                return RedirectToAction("Checkout", new { roomId = roomId });
            }

            var room = await _context.Rooms.Include(r => r.RoomTypes).FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null || !room.IsAvailable)
            {
                TempData["Error"] = "Sorry, this room is no longer available.";
                return RedirectToAction("Index", "Home");
            }

            var isTaken = await _context.Bookings
                .AnyAsync(b => b.RoomId == roomId && b.Status != "Cancelled" &&
                               ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                                (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                                (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));

            if (isTaken)
            {
                TempData["Error"] = "This room is already booked for the selected dates.";
                return RedirectToAction("Checkout", new { roomId = roomId });
            }

            var nights = (int)Math.Ceiling((checkOut - checkIn).TotalDays);
            if (nights == 0) nights = 1;
            var totalPrice = nights * room.RoomTypes.PricePerNight;

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userProfile == null) return RedirectToAction("Register", "Account");

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
                    room.IsAvailable = false;
                    room.Status = "Booked";
                    _context.Rooms.Update(room);
                    await _context.SaveChangesAsync();

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
                                        Description = $"Stay from {checkIn:dd/MM} to {checkOut:dd/MM}"
                                    },
                                },
                                Quantity = 1,
                            },
                        },
                        Mode = "payment",
                        SuccessUrl = domain + $"/Booking/PaymentSuccess?bookingId={booking.BookingId}&paymentMethod={paymentMethod}",
                        CancelUrl = domain + $"/Booking/Checkout?roomId={roomId}",
                    };

                    var service = new SessionService();
                    Session session = service.Create(options);

                    return Redirect(session.Url);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing booking: {ex.Message}";
                return RedirectToAction("Checkout", new { roomId = roomId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(int bookingId, string paymentMethod)
        {
            
            var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Confirmed";
            _context.Bookings.Update(booking);

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = booking.TotalPrice,
                PaymentDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Completed",
                TransactionRefrence = Guid.NewGuid().ToString() // ده بنجيبه من Stripe
            };
            _context.Payments.Add(payment);

            var room = booking.Room;
            room.IsAvailable = false;
            room.Status = "Booked";
            _context.Rooms.Update(room);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment Successful! Your booking is confirmed.";
            return RedirectToAction("MyBookings");
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