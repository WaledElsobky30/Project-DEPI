using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookifyHotel.Model
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }

        public string? PaymentMethod { get; set; } 

        public DateTime PaymentDate { get; set; }

        public string ?PaymentStatus { get; set; }    

        public string? TransactionRefrence { get; set; }

        public int BookingId { get; set; }    // Foreign key to Booking

        public virtual Booking? Booking { get; set; }


    }
}
