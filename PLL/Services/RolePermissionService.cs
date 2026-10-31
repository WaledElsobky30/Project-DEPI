using BookifyHotel.Model;
using DAL.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    public class RolePermissionService : BaseService<RolePermission>
    {
        public RolePermissionService(IGenericRepository<RolePermission> repo) : base(repo) { }
    }

}
