using BookifyHotel.Model;
using DAL.DataBase;

namespace PLL.Services
{
    public class UserService : BaseService<UserProfile>
    {
        public UserService(IGenericRepository<UserProfile> repo) : base(repo)
        {
        }

        public UserProfile? GetByEmail(string email)
        {
            return _repo.FirstOrDefault(u => u.Email == email);
        }

        // جلب مستخدم بالهاتف
        public UserProfile? GetUserByPhone(string phone)
        {
            return _repo.FirstOrDefault(u => u.PhoneNumber == phone);
        }

        // جلب عدد المستخدمين
        public int GetUsersCount()
        {
            return _repo.Count();
        }
    }
}
