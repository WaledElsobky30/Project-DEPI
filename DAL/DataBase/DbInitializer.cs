using BookifyHotel.Data;
using BookifyHotel.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.DataBase
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BookifyHotelDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            await dbContext.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminRole = await roleManager.FindByNameAsync("Admin");
            var userRole = await roleManager.FindByNameAsync("User");

            // 2. Seed Admin User
            await EnsureUserAsync(
                dbContext,
                userManager,
                email: "admin@bookifyhotel.com",
                password: "Admin123",
                fullName: "System Administrator",
                phone: "+1 (555) 100-0001",
                role: adminRole);

            // 3. Seed Sample Guests
            var guest1 = await EnsureUserAsync(
                dbContext,
                userManager,
                email: "sarah.johnson@example.com",
                password: "User123",
                fullName: "Sarah Johnson",
                phone: "+1 (555) 234-5678",
                role: userRole);

            var guest2 = await EnsureUserAsync(
                dbContext,
                userManager,
                email: "michael.chen@example.com",
                password: "User123",
                fullName: "Michael Chen",
                phone: "+1 (555) 345-6789",
                role: userRole);

            var guest3 = await EnsureUserAsync(
                dbContext,
                userManager,
                email: "emily.rodriguez@example.com",
                password: "User123",
                fullName: "Emily Rodriguez",
                phone: "+1 (555) 456-7890",
                role: userRole);

            var guest4 = await EnsureUserAsync(
                dbContext,
                userManager,
                email: "omar.hassan@example.com",
                password: "User123",
                fullName: "Omar Hassan",
                phone: "+1 (555) 567-8901",
                role: userRole);

            // 4. Seed Room Types if empty
            if (!await dbContext.RoomTypes.AnyAsync())
            {
                var roomTypes = new List<RoomType>
                {
                    new RoomType
                    {
                        Name = "Standard City Room",
                        PricePerNight = 140m,
                        Capacity = 2,
                        Description = "Thoughtfully crafted haven with bespoke furnishings, soundproof floor-to-ceiling windows, rainfall shower, and ultra-fast Wi-Fi."
                    },
                    new RoomType
                    {
                        Name = "Deluxe Ocean View",
                        PricePerNight = 230m,
                        Capacity = 2,
                        Description = "Elevated coastal elegance featuring a private balcony, plush king bedding, Italian marble bath, and curated ambient lighting."
                    },
                    new RoomType
                    {
                        Name = "Executive Junior Suite",
                        PricePerNight = 340m,
                        Capacity = 3,
                        Description = "Refined luxury suite with a dedicated living parlor, artisanal minibar, Nespresso station, and sweeping skyline vistas."
                    },
                    new RoomType
                    {
                        Name = "Family Garden Suite",
                        PricePerNight = 410m,
                        Capacity = 4,
                        Description = "Generous multi-space retreat overlooking private gardens, featuring dual queen beds, lounge seating, and family-friendly amenities."
                    },
                    new RoomType
                    {
                        Name = "Royal Presidential Suite",
                        PricePerNight = 680m,
                        Capacity = 4,
                        Description = "Our signature top-floor sanctuary with a private panoramic terrace, soaking jacuzzi, bespoke lounge, and 24/7 butler service."
                    }
                };

                dbContext.RoomTypes.AddRange(roomTypes);
                await dbContext.SaveChangesAsync();
            }

            var allRoomTypes = await dbContext.RoomTypes.OrderBy(rt => rt.PricePerNight).ToListAsync();

            // 5. Seed at least 12 Rooms if empty
            if (!await dbContext.Rooms.AnyAsync() && allRoomTypes.Count >= 5)
            {
                var standardType = allRoomTypes[0];
                var deluxeType = allRoomTypes[1];
                var executiveType = allRoomTypes[2];
                var familyType = allRoomTypes[3];
                var royalType = allRoomTypes[4];

                var rooms = new List<Room>
                {
                    new Room
                    {
                        RoomNumber = 101,
                        Floor = 1,
                        RoomTypeId = standardType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 102,
                        Floor = 1,
                        RoomTypeId = standardType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1590490360182-c33d57733427?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 103,
                        Floor = 1,
                        RoomTypeId = standardType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1566665797739-1674de7a421a?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 201,
                        Floor = 2,
                        RoomTypeId = deluxeType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1618773928121-c32242e63f39?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 202,
                        Floor = 2,
                        RoomTypeId = deluxeType.RoomTypeId,
                        Status = "Booked",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 203,
                        Floor = 2,
                        RoomTypeId = deluxeType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1591088398332-8a7791972843?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 301,
                        Floor = 3,
                        RoomTypeId = executiveType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 302,
                        Floor = 3,
                        RoomTypeId = executiveType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1595576508898-0ad5c879a061?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 401,
                        Floor = 4,
                        RoomTypeId = familyType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1566195992011-5f6b21e539aa?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 402,
                        Floor = 4,
                        RoomTypeId = familyType.RoomTypeId,
                        Status = "Maintenance",
                        IsAvailable = false,
                        ImageUrl = "https://images.unsplash.com/photo-1540518614846-7ede433c4574?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 501,
                        Floor = 5,
                        RoomTypeId = royalType.RoomTypeId,
                        Status = "Available",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1611892440504-42a792e24d32?auto=format&fit=crop&w=1200&q=80"
                    },
                    new Room
                    {
                        RoomNumber = 502,
                        Floor = 5,
                        RoomTypeId = royalType.RoomTypeId,
                        Status = "Booked",
                        IsAvailable = true,
                        ImageUrl = "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?auto=format&fit=crop&w=1200&q=80"
                    }
                };

                dbContext.Rooms.AddRange(rooms);
                await dbContext.SaveChangesAsync();
            }

            // 6. Seed Sample Past, Active, and Future Bookings if empty
            if (!await dbContext.Bookings.AnyAsync())
            {
                var seededRooms = await dbContext.Rooms.Include(r => r.RoomTypes).OrderBy(r => r.RoomNumber).ToListAsync();
                if (seededRooms.Count >= 6)
                {
                    var today = DateTime.Today;

                    var sampleBookings = new List<Booking>
                    {
                        // Past Completed Booking 1
                        new Booking
                        {
                            UserProfileId = guest1.Id,
                            UserId = guest1.UserId,
                            RoomId = seededRooms[0].RoomId, // 101
                            CheckInDate = today.AddDays(-18),
                            CheckOutDate = today.AddDays(-15),
                            BookingDate = today.AddDays(-25),
                            TotalPrice = seededRooms[0].RoomTypes.PricePerNight * 3,
                            Status = "Completed"
                        },
                        // Past Completed Booking 2 (earlier this month for Monthly Revenue)
                        new Booking
                        {
                            UserProfileId = guest2.Id,
                            UserId = guest2.UserId,
                            RoomId = seededRooms[6].RoomId, // 301
                            CheckInDate = today.AddDays(-7),
                            CheckOutDate = today.AddDays(-3),
                            BookingDate = today.AddDays(-12),
                            TotalPrice = seededRooms[6].RoomTypes.PricePerNight * 4,
                            Status = "Completed"
                        },
                        // Active Today Booking 1 (Check-in Today -> counts toward Today's Bookings & Monthly Revenue)
                        new Booking
                        {
                            UserProfileId = guest3.Id,
                            UserId = guest3.UserId,
                            RoomId = seededRooms[4].RoomId, // 202
                            CheckInDate = today,
                            CheckOutDate = today.AddDays(3),
                            BookingDate = today.AddDays(-2),
                            TotalPrice = seededRooms[4].RoomTypes.PricePerNight * 3,
                            Status = "Confirmed"
                        },
                        // Active Today Booking 2 (Royal Suite check-in Today)
                        new Booking
                        {
                            UserProfileId = guest4.Id,
                            UserId = guest4.UserId,
                            RoomId = seededRooms.Last().RoomId, // 502
                            CheckInDate = today,
                            CheckOutDate = today.AddDays(4),
                            BookingDate = today.AddDays(-5),
                            TotalPrice = seededRooms.Last().RoomTypes.PricePerNight * 4,
                            Status = "Confirmed"
                        },
                        // Future Confirmed Booking
                        new Booking
                        {
                            UserProfileId = guest1.Id,
                            UserId = guest1.UserId,
                            RoomId = seededRooms[8].RoomId, // 401
                            CheckInDate = today.AddDays(5),
                            CheckOutDate = today.AddDays(9),
                            BookingDate = today.AddDays(-1),
                            TotalPrice = seededRooms[8].RoomTypes.PricePerNight * 4,
                            Status = "Confirmed"
                        },
                        // Future Pending Booking
                        new Booking
                        {
                            UserProfileId = guest2.Id,
                            UserId = guest2.UserId,
                            RoomId = seededRooms[10].RoomId, // 501
                            CheckInDate = today.AddDays(12),
                            CheckOutDate = today.AddDays(15),
                            BookingDate = today,
                            TotalPrice = seededRooms[10].RoomTypes.PricePerNight * 3,
                            Status = "Pending"
                        }
                    };

                    dbContext.Bookings.AddRange(sampleBookings);
                    await dbContext.SaveChangesAsync();

                    // Seed corresponding Payments for Confirmed/Completed bookings
                    var payments = sampleBookings
                        .Where(b => b.Status == "Confirmed" || b.Status == "Completed")
                        .Select((b, idx) => new Payment
                        {
                            BookingId = b.BookingId,
                            Amount = b.TotalPrice,
                            PaymentDate = b.BookingDate.AddMinutes(15),
                            PaymentMethod = idx % 2 == 0 ? "Credit Card" : "PayPal",
                            PaymentStatus = "Completed",
                            TransactionRefrence = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{1000 + idx}"
                        })
                        .ToList();

                    dbContext.Payments.AddRange(payments);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private static async Task<UserProfile> EnsureUserAsync(
            BookifyHotelDbContext dbContext,
            UserManager<AppUser> userManager,
            string email,
            string password,
            string fullName,
            string phone,
            IdentityRole? role)
        {
            var appUser = await userManager.FindByEmailAsync(email);
            if (appUser == null)
            {
                appUser = new AppUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = phone,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(appUser, password);
                if (createResult.Succeeded && role?.Name != null)
                {
                    await userManager.AddToRoleAsync(appUser, role.Name);
                }
            }

            var profile = await dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == appUser.Id);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = appUser.Id,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phone,
                    ProfilePhoto = "default.png",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                dbContext.UserProfiles.Add(profile);
                await dbContext.SaveChangesAsync();
            }

            if (role != null)
            {
                var hasUserRole = await dbContext.User_Roles.AnyAsync(ur => ur.UserProfileId == profile.Id && ur.RoleId == role.Id);
                if (!hasUserRole)
                {
                    dbContext.User_Roles.Add(new User_Role
                    {
                        UserProfileId = profile.Id,
                        RoleId = role.Id
                    });
                    await dbContext.SaveChangesAsync();
                }
            }

            return profile;
        }
    }
}
