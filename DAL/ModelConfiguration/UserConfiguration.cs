using BookifyHotel.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookifyHotel.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(u => u.Id); 
           
            
            builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
            builder.Property(u => u.PhoneNumber).HasMaxLength(15);
            //builder.Property(u => u.PasswordHash).IsRequired();

        }
    }
}
