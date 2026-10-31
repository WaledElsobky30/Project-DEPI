using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_DEPI_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly BookingService _bookingService;
        private readonly UserRoleService _userRoleService;
        private readonly ReservationCartService _reservationCartService;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly BookifyHotelDbContext _context;

        public UserController(
            UserService userService,
            UserRoleService userRoleService,
            ReservationCartService reservationCartService,
            BookingService bookingService,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            BookifyHotelDbContext context)
        {
            _userService = userService;
            _reservationCartService = reservationCartService;
            _bookingService = bookingService;
            _userRoleService = userRoleService;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        #region Admin Dashboard CRUD Operations

        public IActionResult Index()
        {
            var users = _userService.GetAll();
            var rolesByUser = _userRoleService.GetAllUserRoleNamesByProfileId();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roleNames = rolesByUser.TryGetValue(user.Id, out var roles) && roles.Any()
                    ? roles
                    : new List<string> { "User" };

                userViewModels.Add(new UserViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roleNames,
                    RoleNames = string.Join(", ", roleNames)
                });
            }
            return View(userViewModels);
        }

        public IActionResult Details(int id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();

            ViewBag.BookingsCount = _bookingService.GetUserBookingsCount(id);
            ViewBag.ReservationsCount = _reservationCartService.GetUserReservationsCount(id);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && user.UserId == currentUser.Id)
            {
                TempData["Error"] = "You cannot delete your own administrator account.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var identityUserId = user.UserId;
                _userService.Delete(id);

                if (!string.IsNullOrEmpty(identityUserId))
                {
                    var identityUser = await _userManager.FindByIdAsync(identityUserId);
                    if (identityUser != null)
                    {
                        await _userManager.DeleteAsync(identityUser);
                    }
                }
                TempData["Success"] = "User deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete this user because they have active bookings.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. التحقق من الإيميل
                        var existingUser = await _userManager.FindByEmailAsync(model.Email);
                        if (existingUser != null)
                        {
                            ModelState.AddModelError("Email", "Email already exists!");
                            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                            return View(model);
                        }

                        // 2. إنشاء Identity User
                        var identityUser = new AppUser
                        {
                            UserName = model.Email,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            EmailConfirmed = true
                        };

                        var result = await _userManager.CreateAsync(identityUser, model.Password);

                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                                ModelState.AddModelError("", error.Description);

                            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                            return View(model);
                        }

                        // 3. إضافة الرول في Identity
                        if (!string.IsNullOrEmpty(model.UserRole))
                        {
                            if (!await _roleManager.RoleExistsAsync(model.UserRole))
                                await _roleManager.CreateAsync(new IdentityRole(model.UserRole));

                            await _userManager.AddToRoleAsync(identityUser, model.UserRole);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(identityUser, "User");
                            model.UserRole = "User";
                        }

                        // 4. إنشاء UserProfile
                        var userProfile = new UserProfile
                        {
                            UserId = identityUser.Id,
                            FullName = model.FullName,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber
                        };
                        _userService.Create(userProfile);

                        // 5. ربط الرول في جدولنا الخاص User_Roles
                        var role = await _roleManager.FindByNameAsync(model.UserRole);
                        if (role != null)
                        {
                            _userRoleService.ReplaceUserRoles(userProfile.Id, role.Id);
                        }

                        await transaction.CommitAsync();
                        TempData["Success"] = $"User {model.FullName} created successfully!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Error: {ex.Message}");
                    }
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View(model);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && user.UserId == currentUser.Id)
            {
                TempData["Warning"] = "Edit your own profile from Account Settings.";
                return RedirectToAction("Index");
            }

            // جلب الرول الحالي
            var identityUser = await _userManager.FindByIdAsync(user.UserId);
            var currentRole = "User";
            if (identityUser != null)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                currentRole = roles.FirstOrDefault() ?? "User";
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IdentityUserId = user.UserId,
                UserRole = currentRole
            };

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", currentRole);

            return View(model);
        }

        // POST: User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userProfile = _userService.GetById(model.Id);
                    if (userProfile == null) return NotFound();

                    // تحديث البروفايل
                    userProfile.FullName = model.FullName;
                    userProfile.Email = model.Email;
                    userProfile.PhoneNumber = model.PhoneNumber;
                    _userService.Update(userProfile);

                    // تحديث Identity
                    if (!string.IsNullOrEmpty(userProfile.UserId))
                    {
                        var identityUser = await _userManager.FindByIdAsync(userProfile.UserId);
                        if (identityUser != null)
                        {
                            identityUser.Email = model.Email;
                            identityUser.UserName = model.Email;
                            identityUser.PhoneNumber = model.PhoneNumber;
                            await _userManager.UpdateAsync(identityUser);

                            // تحديث الباسورد لو موجود
                            if (!string.IsNullOrEmpty(model.NewPassword))
                            {
                                var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                                await _userManager.ResetPasswordAsync(identityUser, token, model.NewPassword);
                            }

                            // تحديث الرول
                            var currentRoles = await _userManager.GetRolesAsync(identityUser);
                            var currentRole = currentRoles.FirstOrDefault();

                            if (model.UserRole != currentRole)
                            {
                                await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                                await _userManager.AddToRoleAsync(identityUser, model.UserRole);

                                // تحديث جدولنا الخاص User_Roles مع حذف الأدوار القديمة أولاً
                                var newRole = await _roleManager.FindByNameAsync(model.UserRole);
                                if (newRole != null)
                                {
                                    _userRoleService.ReplaceUserRoles(userProfile.Id, newRole.Id);
                                }
                            }
                        }
                    }

                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", model.UserRole);
            return View(model);
        }

        #endregion
    }
}
