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
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.RoomId);
            builder.Property(r => r.RoomNumber)
                .IsRequired();
            builder.Property(r => r.Floor).IsRequired(); 
            builder.Property(r => r.Status)
                .IsRequired()
                .HasMaxLength(50);

        }
    }
}
