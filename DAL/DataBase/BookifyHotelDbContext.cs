using BookifyHotel.Configuration;
using BookifyHotel.Model;
using BookifyHotel.ModelConfiguration;
using DAL.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookifyHotel.Data
{
    public class BookifyHotelDbContext : IdentityDbContext<AppUser>
    {
        public BookifyHotelDbContext()
        {
        }

        public BookifyHotelDbContext(DbContextOptions<BookifyHotelDbContext> option)
            : base(option)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }    
        //public DbSet<Role> AppRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<User_Role> User_Roles { get; set; }
        public DbSet<RolePermission> Role_Permissions { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ReservationCart> ReservationCarts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            modelBuilder.Entity<UserProfile>().Ignore("UserID");
            modelBuilder.ApplyConfiguration(new ReservationCartConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new BookingConfiguration());
            modelBuilder.ApplyConfiguration(new RoomTypeConfiguration());
            modelBuilder.ApplyConfiguration(new RoomConfiguration());

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<User_Role>()
                .HasKey(ur => new { ur.UserProfileId, ur.RoleId });

            //modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            //modelBuilder.ApplyConfiguration(new RoleConfiguration());

            // 🛑 التعديل هنا: وقفنا السطر ده عشان كان بيتعارض مع الكود اللي تحت
            // modelBuilder.ApplyConfiguration(new UserConfiguration());


            // ❌ احذف هذه السطور
            // modelBuilder.ApplyConfiguration(new RoleConfiguration());
            // modelBuilder.ApplyConfiguration(new PermissionConfiguration());

            // ✅ إعداد العلاقات الجديدة
            modelBuilder.Entity<User_Role>()
                .HasKey(ur => new { ur.UserProfileId, ur.RoleId });

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // إعداد العلاقة بين UserProfile و AppUser
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.IdentityUser)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId)
                .IsRequired();


            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");


            // ربط UserProfile بـ IdentityUser
            //modelBuilder.Entity<UserProfile>()
            //    .HasOne(up => up.IdentityUser)
            //    .WithOne()
            //    .HasForeignKey<UserProfile>(up => up.IdentityUserId)
            //    .IsRequired();




            modelBuilder.Entity<Booking>()
        .Property(b => b.TotalPrice)
        .HasPrecision(18, 2);

            modelBuilder.Entity<ReservationCart>()
                .Property(r => r.PricePreview)
                .HasPrecision(18, 2);


        }
    }
}