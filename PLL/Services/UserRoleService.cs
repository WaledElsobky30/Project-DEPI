using BookifyHotel.Model;
using DAL.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace PLL.Services
{
    public class UserRoleService : BaseService<User_Role>
    {
        public UserRoleService(IGenericRepository<User_Role> repo) : base(repo) { }

        public List<User_Role> GetUserRoles(int userId)
        {
            return _repo.Query()
                .Include(ur => ur.Role)
                .Where(ur => ur.UserProfileId == userId)
                .ToList();
        }

        public Dictionary<int, List<string>> GetAllUserRoleNamesByProfileId()
        {
            return _repo.Query()
                .Include(ur => ur.Role)
                .Where(ur => ur.Role != null && ur.Role.Name != null)
                .AsEnumerable()
                .GroupBy(ur => ur.UserProfileId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ur => ur.Role!.Name!).Distinct().ToList());
        }

        public void ReplaceUserRoles(int userProfileId, string roleId)
        {
            var existing = _repo.Find(ur => ur.UserProfileId == userProfileId).ToList();
            if (existing.Any())
            {
                _repo.RemoveRange(existing);
            }

            _repo.Add(new User_Role
            {
                UserProfileId = userProfileId,
                RoleId = roleId
            });
            _repo.Save();
        }
    }
}
