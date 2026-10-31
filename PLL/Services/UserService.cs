using BookifyHotel.Data;
using BookifyHotel.Model;
using DAL.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class UserService : BaseService<UserProfile>
    {
        public BookifyHotelDbContext _context = new BookifyHotelDbContext();

        public UserService(IGenericRepository<UserProfile> repo) : base(repo)
        {
        }

        public UserProfile? GetByEmail(string email)
        {
            return _repo.GetAll().FirstOrDefault(u => u.Email == email);
        }




        //public string GetUserRole(int userId)
        //{
        //    try
        //    {
        //        // الطريقة الأولى: باستخدام Join (آمنة)
        //        var role = _context.User_Roles
        //            .Where(ur => ur.UserProfileId == userId)
        //            .Join(_context.AppRoles,
        //                  ur => ur.RoleId,
        //                  r => r.RoleId,
        //                  (ur, r) => r.RoleName)
        //            .FirstOrDefault();

        //        return role ?? "UserProfile"; // إذا مفيش رول يرجع "UserProfile" كقيمة افتراضية
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        // _logger.LogError(ex, "Error getting user role for user {UserId}", userId);
        //        return "UserProfile"; // قيمة افتراضية في حالة الخطأ
        //    }
        //}







        // جلب مستخدم بالهاتف
        public UserProfile? GetUserByPhone(string phone)
        {
            return _repo.GetAll()
                .FirstOrDefault(u => u.PhoneNumber == phone);
        }

        // التحقق من صحة كلمة المرور
        //public bool VerifyPassword(string email, string password)
        //{
        //    var user = GetByEmail(email);
        //    // هنا يجب استخدام hashing للتحقق
        //    return user != null && user.PasswordHash == password; // في الواقع يجب مقارنة الـ hash
        //}

        // تحديث بيانات المستخدم
        //public bool UpdateUserProfile(int userId, string firstName, string lastName, string phone)
        //{
        //    var user = _repo.GetById(userId);
        //    if (user == null) return false;

        //    user.FirstName = firstName;
        //    user.LastName = lastName;
        //    user.PhoneNumber = phone;

        //    _repo.Update(user);
        //    _repo.Save();
        //    return true;
        //}

        // جلب عدد المستخدمين
        public int GetUsersCount()
        {
            return _repo.GetAll().Count();
        }














    }

}
