namespace BookifyHotel.Model
{
    public class Room
    {
        public int RoomId { get; set; } 
        public int RoomNumber { get; set; }

        public int Floor { get; set; } 

        public string Status { get; set; }  = string.Empty;

        public int RoomTypeId { get; set; }

        public virtual RoomType RoomTypes { get; set; } = null!;

        public virtual ICollection<Booking> Bookings { get; set; } = null!;  

        public virtual ICollection<ReservationCart> ReservationCarts { get; set; } = null!;
        public bool IsAvailable { get; set; }

       public string ImageUrl { get; set; } = string.Empty; 
    }
}
