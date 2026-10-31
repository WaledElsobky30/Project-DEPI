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
    public class RoomService : BaseService<Room>
    {
        public BookifyHotelDbContext _context ;
        public RoomService(IGenericRepository<Room> repo ,BookifyHotelDbContext context ) : base(repo)
        {
            _context = context;
        }

        //public IEnumerable<Room> GetAvailableRooms()
        //{
        //    return _repo.GetAll()
        //                .Where(r => r.Status == "Available")
        //                .ToList();
        //}


        // جلب غرفة برقمها
        public Room? GetRoomByNumber(int roomNumber)
        {
            return _repo.GetAll()
                .FirstOrDefault(r => r.RoomNumber == roomNumber);
        }

        // جلب الغرف المتاحة
        public IEnumerable<Room> GetAvailableRooms()
        {
            return _repo.GetAll()
                .Where(r => r.IsAvailable && r.Status == "Available")
                .ToList();
        }

        // جلب الغرف بنوع معين
        public IEnumerable<Room> GetRoomsByType(int roomTypeId)
        {
            return _repo.GetAll()
                .Where(r => r.RoomTypeId == roomTypeId && r.IsAvailable)
                .ToList();
        }

        // جلب الغرف في طابق معين
        public IEnumerable<Room> GetRoomsByFloor(int floor)
        {
            return _repo.GetAll()
                .Where(r => r.Floor == floor)
                .ToList();
        }

        // جلب غرفة مع تفاصيل نوعها
        public Room ?GetRoomWithType(int roomId)
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

        // البحث عن غرف متاحة في تاريخ معين
        public IEnumerable<Room> GetAvailableRoomsForDates(DateTime checkIn, DateTime checkOut)
        {
            // نحتاج للتحقق من الحجوزات
            var bookedRoomIds = _context.Bookings
                .Where(b => b.Status != "Cancelled" &&
                           ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                            (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                            (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)))
                .Select(b => b.RoomId)
                .ToList();

            return _repo.GetAll()
                .Where(r => !bookedRoomIds.Contains(r.RoomId) && r.IsAvailable)
                .ToList();
        }








    }

}
