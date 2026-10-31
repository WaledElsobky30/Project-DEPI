using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class RoomTypeService : BaseService<RoomType>
    {
        public BookifyHotelDbContext _context;
        public RoomTypeService(IGenericRepository<RoomType> repo , BookifyHotelDbContext context) : base(repo) 
        { 
            _context = context;
        }


        // جلب نوع غرفة باسمه
        public RoomType? GetRoomTypeByName(string name)
        {
            return _repo.GetAll()
                .FirstOrDefault(rt => rt.Name == name);
        }

        // جلب السعر لنوع غرفة
        public decimal GetRoomTypePrice(int roomTypeId)
        {
            var roomType = _repo.GetById(roomTypeId);
            return roomType?.PricePerNight ?? 0;
        }

        // جلب أنواع الغرف مع عدد الغرف المتاحة من كل نوع
        public IEnumerable<RoomTypeViewModel> GetRoomTypesWithAvailability()
        {
            var roomTypes = _repo.GetAll().ToList();
            var result = new List<RoomTypeViewModel>();

            foreach (var roomType in roomTypes)
            {
                // عد الغرف المتاحة من هذا النوع
                int availableRoomsCount = _context.Rooms
                    .Count(r => r.RoomTypeId == roomType.RoomTypeId &&
                               r.IsAvailable && r.Status == "Available");

                var viewModel = new RoomTypeViewModel
                {
                    RoomTypeId = roomType.RoomTypeId,
                    Name = roomType.Name,
                   
                    PricePerNight = roomType.PricePerNight,
                   
                  
                    AvailableRoomsCount = availableRoomsCount,
                    
                };

                result.Add(viewModel);
            }
            return result;
        }

        // جلب أنواع الغرف المتاحة
        public IEnumerable<RoomType> GetAvailableRoomTypes()
        {
            var availableRoomTypes = _context.Rooms
                .Where(r => r.IsAvailable)
                .Select(r => r.RoomTypeId)
                .Distinct()
                .ToList();

            return _repo.GetAll()
                .Where(rt => availableRoomTypes.Contains(rt.RoomTypeId))
                .ToList();
        }


    }

}
