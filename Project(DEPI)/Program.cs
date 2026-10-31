using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PLL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// أضف هذا السطر
builder.Services.AddDbContext<BookifyHotelDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Project(DEPI)")
              .EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ).UseLazyLoadingProxies());

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<RoomTypeService>();
builder.Services.AddScoped<RolePermissionService>();
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<ReservationCartService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<BookingService>();

// Identity registration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // تهيئة الباسورد والقواعد حسب احتياجك
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<BookifyHotelDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/Login";
    options.AccessDeniedPath = "/Home/Index";
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // مهم
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BasePage}/{action=Index}/{id?}")
    .WithStaticAssets();

// Automatically migrate and seed realistic dummy data if the database is empty
await DbInitializer.SeedAsync(app.Services);

app.Run();
