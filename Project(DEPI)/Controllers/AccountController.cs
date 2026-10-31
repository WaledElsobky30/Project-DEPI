using BookifyHotel.Data;
using BookifyHotel.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BookifyHotel.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly BookifyHotelDbContext _context;

        public RegisterController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            BookifyHotelDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel newUser)
        {
            if (!ModelState.IsValid)
                return View(newUser);

            var appUser = new AppUser
            {
                UserName = newUser.Email,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber
            };

            var result = await _userManager.CreateAsync(appUser, newUser.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(newUser);
            }

            try
            {
                // تعيين الرول الافتراضي للمستخدم الجديد
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                await _userManager.AddToRoleAsync(appUser, "User");

                var profile = new UserProfile
                {
                    UserId = appUser.Id,
                    FullName = newUser.FullName,
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber,
                    ProfilePhoto = "default.png",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // هنا بحط البيانات في الداتا بيز
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();

                var userRole = await _roleManager.FindByNameAsync("User");
                if (userRole != null)
                {
                    _context.User_Roles.Add(new User_Role
                    {
                        UserProfileId = profile.Id,
                        RoleId = userRole.Id
                    });
                    await _context.SaveChangesAsync();
                }

                //وهنا بسجل على طول 
                await _signInManager.SignInAsync(appUser, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                //هنا بحذف اليوزر لو حصل خطأ في اضافة البيانات التانية
                await _userManager.DeleteAsync(appUser);

                // عرض رسالة الخطأ بالتفصيل للمساعدة في التتبع
                ModelState.AddModelError("", $"حدث خطأ أثناء استكمال البيانات: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", $"تفاصيل الخطأ: {ex.InnerException.Message}");
                }

                return View(newUser);
            }
        }
    }
}
