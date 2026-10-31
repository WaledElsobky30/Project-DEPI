using BookifyHotel.Model;
using System.ComponentModel.DataAnnotations.Schema;

public class ReservationCart
{
    public int ReservationCartId { get; set; }
    public int UserProfileId { get; set; }

    public int RoomId { get; set; } // ✅ يجب أن يكون هذا فقط

    public decimal PricePreview { get; set; }
    public DateTime CheckinDate { get; set; }
    public DateTime CheckoutDate { get; set; }

    // ✅ العلاقات
    [ForeignKey("UserProfileId")]
    public virtual UserProfile UserProfile { get; set; } = null!;

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; } = null!;
}
