using BookifyHotel.Model;
using Microsoft.AspNetCore.Mvc;
using PLL.Services;

namespace Project_DEPI_.Controllers
{
    public class RoomController : Controller
    {
        private readonly RoomService _roomService;
        private readonly RoomTypeService _roomTypeService;

        public RoomController(RoomService roomService, RoomTypeService roomTypeService)
        {
            _roomService = roomService;
            _roomTypeService = roomTypeService;
        }

        #region User Side Operations
        public IActionResult Index()
        {
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            var rooms = _roomService.GetAll();
            return View(rooms);
        }
        public IActionResult Details(int id)
        {
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            var room = _roomService.GetById(id);
            return View(room);

        }
        #endregion


        #region Admin Dashboard  CRUD Operations
        // 
        public IActionResult DetailsToAdmin(int id)
        {    
            var room = _roomService.GetById(id);
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            return View(room);
        }


        public IActionResult Index2()    // Admin View 
        {
            var Room = _roomService.GetAll();
            return View(Room);
        }


        public IActionResult Add()
        {
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            return View(new Room()) ;
        }

        [HttpPost]
        public IActionResult AddSave(Room room)
        {
                _roomService.Create(room);
                return RedirectToAction("Index2");  
        }

        public IActionResult Edit(int id)
        {
           var room = _roomService.GetById(id);
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            return View(room); 
        }
        [HttpPost]
        public IActionResult EditSave(Room room)
        {
            _roomService.Update(room);
            return RedirectToAction("Index2");
        }



        public IActionResult Delete(int id)
        {
            _roomService.Delete(id);
            return RedirectToAction("Index2");
        }

        #endregion

    }
}
