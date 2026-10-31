using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookifyHotel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BookifyHotel.ModelConfiguration
{
    public class PaymentConfiguration:IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.PaymentId);
            builder.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(p => p.PaymentDate)
                .IsRequired()
                 .HasDefaultValueSql("GETDATE()");
            builder.Property(p => p.TransactionRefrence)
                .HasMaxLength(100);
            // Relationship with Booking
            builder.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    

    }
}
