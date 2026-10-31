
using System.ComponentModel.DataAnnotations;

namespace Project_DEPI_.Controllers
{
    public class BookingViewModel
    {
        public int RoomId { get; set; }
        public int RoomNumber { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }

        [Required(ErrorMessage = "Check-in date is required")]
        [Display(Name = "Check-in Date")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required")]
        [Display(Name = "Check-out Date")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Number of guests is required")]
        [Range(1, 4, ErrorMessage = "Guests must be between 1 and 4")]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; }

        public string SpecialRequests { get; set; } = string.Empty;

        public DateTime MinCheckInDate { get; set; }
        public DateTime MaxCheckOutDate { get; set; }
        public string? FullName { get; internal set; }
        public int BookingId { get; internal set; }
        public string Status { get; internal set; }
        public decimal TotalPrice { get; internal set; }
    }
}