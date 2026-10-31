using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PLL.Services
{
    public class RoomService : BaseService<Room>
    {
        private readonly BookifyHotelDbContext _context;

        public RoomService(IGenericRepository<Room> repo, BookifyHotelDbContext context) : base(repo)
        {
            _context = context;
        }

        // جلب غرفة برقمها
        public Room? GetRoomByNumber(int roomNumber)
        {
            return _repo.FirstOrDefault(r => r.RoomNumber == roomNumber);
        }

        // جلب الغرف المتاحة
        public IEnumerable<Room> GetAvailableRooms()
        {
            return _repo.Query()
                .Include(r => r.RoomTypes)
                .Where(r => r.IsAvailable && r.Status != "Maintenance")
                .ToList();
        }

        // جلب الغرف بنوع معين
        public IEnumerable<Room> GetRoomsByType(int roomTypeId)
        {
            return _repo.Query()
                .Include(r => r.RoomTypes)
                .Where(r => r.RoomTypeId == roomTypeId && r.IsAvailable)
                .ToList();
        }

        // جلب الغرف في طابق معين
        public IEnumerable<Room> GetRoomsByFloor(int floor)
        {
            return _repo.Find(r => r.Floor == floor);
        }

        // جلب غرفة مع تفاصيل نوعها
        public Room? GetRoomWithType(int roomId)
        {
            return _context.Rooms
                .Include(r => r.RoomTypes)
                .FirstOrDefault(r => r.RoomId == roomId);
        }

        // تحديث حالة الغرفة
        public bool UpdateRoomStatus(int roomId, string status, bool isAvailable)
        {
            var room = _repo.GetById(roomId);
            if (room == null) return false;

            room.Status = status;
            room.IsAvailable = isAvailable;

            _repo.Update(room);
            _repo.Save();
            return true;
        }

        // البحث عن غرف متاحة في تاريخ معين (باستعلام SQL مباشر)
        public IEnumerable<Room> GetAvailableRoomsForDates(DateTime checkIn, DateTime checkOut)
        {
            return _repo.Query()
                .Include(r => r.RoomTypes)
                .Where(r => r.IsAvailable &&
                            !_context.Bookings.Any(b =>
                                b.RoomId == r.RoomId &&
                                b.Status != "Cancelled" &&
                                checkIn < b.CheckOutDate &&
                                checkOut > b.CheckInDate))
                .ToList();
        }
    }
}
