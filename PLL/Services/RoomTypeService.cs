using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.DataBase;
using DAL.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace PLL.Services
{
    public class RoomTypeService : BaseService<RoomType>
    {
        private readonly BookifyHotelDbContext _context;

        public RoomTypeService(IGenericRepository<RoomType> repo, BookifyHotelDbContext context) : base(repo)
        {
            _context = context;
        }

        // جلب نوع غرفة باسمه
        public RoomType? GetRoomTypeByName(string name)
        {
            return _repo.FirstOrDefault(rt => rt.Name == name);
        }

        // جلب السعر لنوع غرفة
        public decimal GetRoomTypePrice(int roomTypeId)
        {
            var roomType = _repo.GetById(roomTypeId);
            return roomType?.PricePerNight ?? 0;
        }

        // جلب أنواع الغرف مع عدد الغرف المتاحة من كل نوع باستعلام واحد بدون N+1
        public IEnumerable<RoomTypeViewModel> GetRoomTypesWithAvailability()
        {
            return _repo.Query()
                .Select(rt => new RoomTypeViewModel
                {
                    RoomTypeId = rt.RoomTypeId,
                    Name = rt.Name,
                    PricePerNight = rt.PricePerNight,
                    AvailableRoomsCount = rt.Rooms.Count(r => r.IsAvailable && r.Status == "Available")
                })
                .ToList();
        }

        // جلب أنواع الغرف المتاحة
        public IEnumerable<RoomType> GetAvailableRoomTypes()
        {
            return _repo.Query()
                .Where(rt => rt.Rooms.Any(r => r.IsAvailable))
                .ToList();
        }
    }
}
