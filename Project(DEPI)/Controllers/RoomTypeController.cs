using BookifyHotel.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLL.Services;
using System;

namespace Project_DEPI_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoomTypeController : Controller
    {
        private readonly RoomTypeService _roomTypeService;

        public RoomTypeController(RoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        #region Admin Dashboard CRUD Operations

        // GET: RoomType/Index
        public IActionResult Index()
        {
            var roomTypes = _roomTypeService.GetAll();
            return View(roomTypes);
        }

        // GET: RoomType/Details/5
        public IActionResult Details(int id)
        {
            var roomType = _roomTypeService.GetById(id);
            if (roomType == null) return NotFound();
            return View(roomType);
        }

        // GET: RoomType/Create
        public IActionResult Create()
        {
            return View(new RoomType());
        }

        // POST: RoomType/CreateSave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSave(RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _roomTypeService.Create(roomType);
                    TempData["Success"] = "Room Type created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating room type: " + ex.Message);
                }
            }
            return View("Create", roomType);
        }

        // GET: RoomType/Edit/5
        public IActionResult Edit(int id)
        {
            var roomType = _roomTypeService.GetById(id);
            if (roomType == null) return NotFound();
            return View(roomType);
        }

        // POST: RoomType/EditSave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSave(RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _roomTypeService.Update(roomType);
                    TempData["Success"] = "Room Type updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating room type: " + ex.Message);
                }
            }
            return View("Edit", roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var roomType = _roomTypeService.GetById(id);
            if (roomType == null)
            {
                TempData["Error"] = "Room Type not found.";
                return RedirectToAction("Index");
            }

            try
            {
                _roomTypeService.Delete(id);
                TempData["Success"] = "Room Type deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete this Room Type because it contains Rooms.";
            }

            return RedirectToAction("Index");
        }

        #endregion
    }
}
