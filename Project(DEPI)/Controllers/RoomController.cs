using BookifyHotel.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLL.Services;
using System;

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
            if (room == null) return NotFound();
            return View(room);
        }
        #endregion

        #region Admin Dashboard CRUD Operations
        [Authorize(Roles = "Admin")]
        public IActionResult DetailsToAdmin(int id)
        {
            var room = _roomService.GetById(id);
            if (room == null) return NotFound();
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            return View(room);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index2() // Admin View
        {
            var rooms = _roomService.GetAll();
            return View(rooms);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            return View(new Room());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult AddSave(Room room)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoomTypes = _roomTypeService.GetAll();
                return View("Add", room);
            }

            _roomService.Create(room);
            TempData["Success"] = "Room created successfully.";
            return RedirectToAction("Index2");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var room = _roomService.GetById(id);
            if (room == null) return NotFound();
            ViewBag.RoomTypes = _roomTypeService.GetAll();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult EditSave(Room room)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoomTypes = _roomTypeService.GetAll();
                return View("Edit", room);
            }

            _roomService.Update(room);
            TempData["Success"] = "Room updated successfully.";
            return RedirectToAction("Index2");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                _roomService.Delete(id);
                TempData["Success"] = "Room deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete this room because it has existing bookings.";
            }
            return RedirectToAction("Index2");
        }
        #endregion
    }
}
