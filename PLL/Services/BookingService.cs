using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class BookingService : BaseService<Booking>
    {
        public BookifyHotelDbContext _context ;
        public BookingService(IGenericRepository<Booking> repo, BookifyHotelDbContext context) : base(repo) 
        {
            _context = context;

        }


        public int GetUserBookingsCount(int userId)
        {
            return _repo.GetAll()
                .Count(b => b.UserProfileId == userId);
        }


        public int GetActiveUserBookingsCount(int userId)
        {
            return _repo.GetAll()
                .Count(b => b.UserProfileId == userId &&
                           (b.Status == "Confirmed" || b.Status == "Completed"));
        }








        // نضيف هذه الدوال:

        // جلب جميع حجوزات مستخدم معين
        public IEnumerable<Booking> GetBookingsByUserId(int userId)
        {
            return _repo.GetAll()
                .Where(b => b.UserProfileId == userId)
                .OrderByDescending(b => b.CheckInDate)
                .ToList();
        }

        // إنشاء حجز جديد
        public async Task<Booking>? CreateBookingAsync(int userId, int roomId, DateTime checkIn, DateTime checkOut, int guests)
        {
            // حساب السعر (هنا نستدعي RoomService للحصول على السعر)
            // لكن أولاً ننشئ الحجز
            var booking = new Booking
            {
                UserProfileId = userId,
                RoomId = roomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = 0, // سيتم حسابه لاحقاً
                Status = "Pending", // أو "Confirmed"
                BookingDate = DateTime.Now
            };

            _repo.Add(booking);
            _repo.Save();
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
        public Booking ?GetBookingWithRoom(int bookingId)
        {
            // نستخدم DbContext للـ Include
            return _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefault(b => b.BookingId == bookingId);
        }

        // التحقق من تواريخ الحجز
        public bool IsRoomAvailableForDates(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return !_repo.GetAll()
                .Any(b => b.RoomId == roomId &&
                         b.Status != "Cancelled" &&
                         ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                          (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                          (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));
        }










    }

}
