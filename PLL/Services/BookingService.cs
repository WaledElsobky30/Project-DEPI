using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class BookingService : BaseService<Booking>
    {
        private readonly BookifyHotelDbContext _context;

        public BookingService(IGenericRepository<Booking> repo, BookifyHotelDbContext context) : base(repo)
        {
            _context = context;
        }

        public int GetUserBookingsCount(int userId)
        {
            return _repo.Count(b => b.UserProfileId == userId);
        }

        public int GetActiveUserBookingsCount(int userId)
        {
            return _repo.Count(b => b.UserProfileId == userId &&
                           (b.Status == "Confirmed" || b.Status == "Completed"));
        }

        // جلب جميع حجوزات مستخدم معين
        public IEnumerable<Booking> GetBookingsByUserId(int userId)
        {
            return _repo.Query()
                .Where(b => b.UserProfileId == userId)
                .OrderByDescending(b => b.CheckInDate)
                .ToList();
        }

        public async Task<List<Booking>> GetAllWithDetailsAsync()
        {
            return await _repo.Query()
                .Include(b => b.UserProfile)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomTypes)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdWithDetailsAsync(int bookingId)
        {
            return await _repo.Query()
                .Include(b => b.UserProfile)
                .Include(b => b.Payments)
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomTypes)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        // إنشاء حجز جديد
        public async Task<Booking> CreateBookingAsync(int userId, int roomId, DateTime checkIn, DateTime checkOut, int guests)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomTypes)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            int nights = Math.Max((int)Math.Ceiling((checkOut - checkIn).TotalDays), 1);
            decimal totalPrice = (room?.RoomTypes?.PricePerNight ?? 0) * nights;

            var booking = new Booking
            {
                UserProfileId = userId,
                RoomId = roomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = totalPrice,
                Status = "Pending",
                BookingDate = DateTime.Now
            };

            _repo.Add(booking);
            await _repo.SaveAsync();
            return booking;
        }

        // تحديث حالة الحجز
        public bool UpdateBookingStatus(int bookingId, string status)
        {
            var booking = _repo.GetById(bookingId);
            if (booking == null) return false;

            booking.Status = status;
            _repo.Update(booking);
            _repo.Save();
            return true;
        }

        // جلب حجز مع بيانات الغرفة (بتضمين العلاقات)
        public Booking? GetBookingWithRoom(int bookingId)
        {
            return _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r.RoomTypes)
                .FirstOrDefault(b => b.BookingId == bookingId);
        }

        // التحقق من تواريخ الحجز (باستعلام SQL مباشر بدلاً من تحميل الجدول بالكامل في الذاكرة)
        public bool IsRoomAvailableForDates(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null)
        {
            return !_repo.Any(b => b.RoomId == roomId &&
                                   (excludeBookingId == null || b.BookingId != excludeBookingId.Value) &&
                                   b.Status != "Cancelled" &&
                                   checkIn < b.CheckOutDate &&
                                   checkOut > b.CheckInDate);
        }

        public async Task<bool> IsRoomAvailableForDatesAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null)
        {
            return !await _repo.AnyAsync(b => b.RoomId == roomId &&
                                              (excludeBookingId == null || b.BookingId != excludeBookingId.Value) &&
                                              b.Status != "Cancelled" &&
                                              checkIn < b.CheckOutDate &&
                                              checkOut > b.CheckInDate);
        }
    }
}
