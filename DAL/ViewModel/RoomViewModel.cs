using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModel
{
    public class RoomViewModel
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Room Number is required")]
        [Display(Name = "Room Number")]
        public int RoomNumber { get; set; }

        [Required(ErrorMessage = "Please select a floor")]
        [Display(Name = "Floor Number")]
        public int Floor { get; set; }

        [Display(Name = "Room Status")]
        public string? Status { get; set; } = "Available"; 

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        // عشان نربط الغرفة بنوعها (Dropdown List)
        [Required(ErrorMessage = "Please select a Room Type")]
        [Display(Name = "Room Type")]
        public int RoomTypeId { get; set; }

        // القائمة اللي هتتملي في الكنترولر عشان تظهر في الفيو
        public IEnumerable<SelectListItem>? RoomTypes { get; set; }

        //  ده أهم جزء عشان رفع الصور (بيستقبل الملف من الفورم)
        [Display(Name = "Room Image")]
        public IFormFile? ImageFile { get; set; }

        // ده عشان نعرض الصورة المتسيفة في الداتابيز (مسار الصورة)
        public string? ImageUrl { get; set; }
    }
}