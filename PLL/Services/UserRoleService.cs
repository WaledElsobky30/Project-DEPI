using BookifyHotel.Model;
using DAL.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class UserRoleService : BaseService<User_Role>
    {
        public UserRoleService(IGenericRepository<User_Role> repo) : base(repo) { }

        public List<User_Role> GetUserRoles(int userId)
        {
            return _repo.GetAll().Where(ur => ur.UserProfileId == userId).ToList();
        }
    }

}
