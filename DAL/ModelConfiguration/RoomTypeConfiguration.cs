using BookifyHotel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookifyHotel.ModelConfiguration
{
    public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> builder)
        {
            builder.HasKey(rt => rt.RoomTypeId);
            builder.Property(rt => rt.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(rt => rt.PricePerNight)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
        }
    

    }
}
