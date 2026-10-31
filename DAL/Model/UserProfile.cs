using DAL.DataBase;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookifyHotel.Model; 

namespace BookifyHotel.Model 
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; 

        [ForeignKey("UserID")] 
        public virtual AppUser? IdentityUser { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Email { get; set; }

        public virtual ICollection<User_Role> User_Roles { get; set; } = new List<User_Role>();

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>(); 

        public virtual ICollection<ReservationCart> ReservationCarts { get; set; } = new List<ReservationCart>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ProfilePhoto { get; set; }
    }
}