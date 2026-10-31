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
    // ReservationCartConfiguration.cs
    public class ReservationCartConfiguration : IEntityTypeConfiguration<ReservationCart>
    {
        public void Configure(EntityTypeBuilder<ReservationCart> builder)
        {
            builder.HasKey(r => r.ReservationCartId);

            builder.HasOne(r => r.UserProfile)
                   .WithMany(u => u.ReservationCarts)
                   .HasForeignKey(r => r.UserProfileId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Room)
                   .WithMany()
                   .HasForeignKey(r => r.RoomId) // ✅ تأكد أنه RoomId واحد
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
