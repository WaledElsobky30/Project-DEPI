using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookifyHotel.Model 
{
    public class Booking
    {
        public int BookingId { get; set; }

        public int UserProfileId { get; set; }
        public int RoomId { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }

        // العلاقات (Relationships)
        public virtual UserProfile UserProfile { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public string UserId { get; set; }
    }
} 